using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM / 효과음 재생 + 볼륨·음소거 저장.
///
/// 클립은 Resources 폴더에서 이름으로 불러온다.
///   Assets/Resources/Sound/BGM/Main.mp3   → GameManager.Sound.PlayBGM("Main");
///   Assets/Resources/Sound/SFX/Click.wav  → GameManager.Sound.PlaySFX("Click");
/// 하위 폴더도 그대로 쓸 수 있다. (Sound/SFX/UI/Click → "UI/Click")
/// 볼륨/음소거는 PlayerPrefs에 저장되어 다음 실행 때 복원된다.
/// </summary>
public class SoundManager : MonoSingleton<SoundManager>
{
    private const string KEY_BGM_VOLUME = "Sound.BgmVolume";
    private const string KEY_SFX_VOLUME = "Sound.SfxVolume";
    private const string KEY_MUTED = "Sound.Muted";

    private const int SFX_POOL_INITIAL = 8;
    private const int SFX_POOL_MAX = 24;
    private const float SFX_MIN_INTERVAL = 0.04f; // 같은 클립이 한 프레임에 겹쳐 터지는 걸 방지
    private const float DEFAULT_BGM_FADE = 1f;

    // BGM은 두 채널을 번갈아 쓰며 크로스페이드한다.
    private class BgmChannel
    {
        public AudioSource Source;
        public string Name;
        public float Level;  // 페이드 배율 0~1
        public float Target; // 목표 배율
        public float Speed;  // 초당 변화량
    }

    private readonly Dictionary<string, AudioClip> bgmCache = new();
    private readonly Dictionary<string, AudioClip> sfxCache = new();
    private readonly HashSet<string> missing = new(); // 없는 클립 경고는 한 번만
    private readonly List<AudioSource> sfxPool = new();
    private readonly Dictionary<AudioClip, float> lastSfxTime = new();

    private BgmChannel[] bgmChannels;
    private int currentBgm;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;
    private bool muted;

    /// <summary>BGM 볼륨 (0~1). 저장됨.</summary>
    public float BgmVolume
    {
        get => bgmVolume;
        set { bgmVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KEY_BGM_VOLUME, bgmVolume); }
    }

    /// <summary>효과음 볼륨 (0~1). 저장됨.</summary>
    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KEY_SFX_VOLUME, sfxVolume); }
    }

    /// <summary>전체 음소거. AudioListener로 처리하므로 다른 AudioSource도 함께 꺼진다. 저장됨.</summary>
    public bool IsMuted
    {
        get => muted;
        set
        {
            if (muted == value) return;
            muted = value;
            AudioListener.volume = muted ? 0f : 1f;
            PlayerPrefs.SetInt(KEY_MUTED, muted ? 1 : 0);
            PlayerPrefs.Save();
            OnMuteChanged?.Invoke(muted);
        }
    }

    public string CurrentBgmName => bgmChannels?[currentBgm].Target > 0f ? bgmChannels[currentBgm].Name : null;

    public event Action<bool> OnMuteChanged;

    // 첫 씬이 로드되면 저장된 음소거 상태를 바로 적용한다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap() => _ = Instance;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return; // 중복 인스턴스는 base에서 파괴 예약됨

        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);

        bgmVolume = PlayerPrefs.GetFloat(KEY_BGM_VOLUME, 1f);
        sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
        muted = PlayerPrefs.GetInt(KEY_MUTED, 0) == 1;
        AudioListener.volume = muted ? 0f : 1f;

        bgmChannels = new BgmChannel[2];
        for (var i = 0; i < bgmChannels.Length; i++)
        {
            var source = CreateSource($"BGM_{i}");
            source.loop = true;
            bgmChannels[i] = new BgmChannel { Source = source };
        }

        for (var i = 0; i < SFX_POOL_INITIAL; i++)
            sfxPool.Add(CreateSource($"SFX_{i}"));
    }

    private AudioSource CreateSource(string objName)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(transform, false);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D
        return source;
    }

    private void Update()
    {
        if (bgmChannels == null) return; 

        var dt = Time.unscaledDeltaTime;
        foreach (var ch in bgmChannels)
        {
            if (ch.Level != ch.Target)
                ch.Level = Mathf.MoveTowards(ch.Level, ch.Target, ch.Speed * dt);

            if (ch.Source.isPlaying)
            {
                ch.Source.volume = ch.Level * bgmVolume;
                if (ch.Level <= 0f && ch.Target <= 0f)
                    ch.Source.Stop();
            }
        }
    }

    // ───────────────────────── BGM ─────────────────────────

    /// <summary>Resources/Sound/BGM/{name} 을 크로스페이드로 재생. 같은 곡이 이미 재생 중이면 무시.</summary>
    public void PlayBGM(string name, float fadeDuration = DEFAULT_BGM_FADE)
    {
        var current = bgmChannels[currentBgm];
        if (current.Name == name && current.Target > 0f) return;

        var clip = LoadClip(bgmCache, GameConstants.Paths.BGM_PATH, name);
        if (clip == null) return;

        FadeTo(current, 0f, fadeDuration);

        currentBgm = (currentBgm + 1) % bgmChannels.Length;
        var next = bgmChannels[currentBgm];
        next.Name = name;
        next.Source.clip = clip;
        next.Source.volume = 0f;
        next.Level = 0f;
        next.Source.Play();
        FadeTo(next, 1f, fadeDuration);
    }

    /// <summary>BGM을 페이드아웃하며 정지. fadeDuration이 0이면 즉시 정지.</summary>
    public void StopBGM(float fadeDuration = DEFAULT_BGM_FADE)
    {
        foreach (var ch in bgmChannels)
        {
            FadeTo(ch, 0f, fadeDuration);
            if (fadeDuration <= 0f) ch.Source.Stop(); // 즉시 정지
        }
    }

    private static void FadeTo(BgmChannel ch, float target, float duration)
    {
        ch.Target = target;
        ch.Speed = duration <= 0f ? float.MaxValue : 1f / duration;
        if (duration <= 0f) ch.Level = target;
    }

    // ───────────────────────── SFX ─────────────────────────

    /// <summary>Resources/Sound/SFX/{name} 효과음 재생.</summary>
    public void PlaySFX(string name, float volumeScale = 1f, float pitch = 1f)
    {
        var clip = LoadClip(sfxCache, GameConstants.Paths.SFX_PATH, name);
        PlaySFX(clip, volumeScale, pitch);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null || muted) return;

        var now = Time.unscaledTime;
        if (lastSfxTime.TryGetValue(clip, out var last) && now - last < SFX_MIN_INTERVAL) return;
        lastSfxTime[clip] = now;

        var source = GetFreeSfxSource();
        if (source == null) return; // 풀이 가득 차면 이번 소리는 생략

        source.clip = clip;
        source.volume = Mathf.Clamp01(volumeScale) * sfxVolume;
        source.pitch = pitch;
        source.Play();
    }

    /// <summary>자주 쓰는 효과음을 미리 메모리에 올려 첫 재생 끊김을 줄인다.</summary>
    public void PreloadSFX(params string[] names)
    {
        foreach (var n in names)
            LoadClip(sfxCache, GameConstants.Paths.SFX_PATH, n);
    }

    private AudioSource GetFreeSfxSource()
    {
        foreach (var s in sfxPool)
            if (!s.isPlaying) return s;

        if (sfxPool.Count >= SFX_POOL_MAX) return null;

        var added = CreateSource($"SFX_{sfxPool.Count}");
        sfxPool.Add(added);
        return added;
    }

    // ───────────────────────── 공통 ─────────────────────────

    public void ToggleMute() => IsMuted = !IsMuted;

    private AudioClip LoadClip(Dictionary<string, AudioClip> cache, string folder, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (cache.TryGetValue(name, out var cached)) return cached;

        var path = folder + name;
        var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            if (missing.Add(path))
                Debug.LogWarning($"[SoundManager] 클립을 찾을 수 없음: Resources/{path}");
            return null;
        }

        cache[name] = clip;
        return clip;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save(); // 볼륨 슬라이더 변경분 저장
    }

    protected override void OnDestroy()
    {
        PlayerPrefs.Save();
        base.OnDestroy();
    }
}
