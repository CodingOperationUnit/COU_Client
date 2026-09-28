using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class SkillFactory
{
    private static readonly Dictionary<int, Type> _skillTypesById = BuildSkillTypeMap();

    public static SkillBase Create( int skillId )
    {
        if( !_skillTypesById.TryGetValue( skillId, out Type skillType ) )
        {
            Debug.LogWarning( $"[SkillFactory] 등록되지 않은 스킬 ID입니다: {skillId}" );
            return null;
        }

        var skill = (SkillBase)Activator.CreateInstance( skillType );

        SkillDataBase.Load();
        SkillData data = SkillDataBase.Get( skillId );
        skill.Initialize( data );

        return skill;
    }

    private static Dictionary<int, Type> BuildSkillTypeMap()
    {
        var map = new Dictionary<int, Type>();

        IEnumerable<Type> skillTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany( assembly => assembly.GetTypes() )
            .Where( type => !type.IsAbstract && typeof( SkillBase ).IsAssignableFrom( type ) );

        foreach( Type type in skillTypes )
        {
            SkillIdAttribute attribute = type.GetCustomAttribute<SkillIdAttribute>();

            if( attribute == null )
            {
                Debug.LogWarning( $"[SkillFactory] {type.Name}에 SkillIdAttribute가 없어 등록을 건너뜁니다." );
                continue;
            }

            if( !map.TryAdd( attribute.SkillId, type ) )
            {
                Debug.LogWarning( $"[SkillFactory] 스킬 ID가 중복되었습니다: {attribute.SkillId} ({type.Name})" );
            }
        }

        return map;
    }
}
