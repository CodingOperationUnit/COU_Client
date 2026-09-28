using System.Collections.Generic;
using UnityEngine;

public static class SkillDataBase
{
    private static Dictionary<int, SkillData> _dataById;

    public static void Load()
    {
        if( _dataById != null )
        {
            return;
        }

        _dataById = new Dictionary<int, SkillData>();

        SkillData[] allData = Resources.LoadAll<SkillData>( "Data/Skills" );

        foreach( SkillData data in allData )
        {
            _dataById[data.SkillId] = data;
        }
    }

    public static SkillData Get( int skillId )
    {
        return _dataById.GetValueOrDefault( skillId );
    }
}
