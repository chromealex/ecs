using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class UnitInfos : ScriptableObject {

    public UnitInfo[] units;

    public UnitInfo GetInfo(int unitTypeId) {

        return this.units.FirstOrDefault(x => x.unitTypeId == unitTypeId);

    }

}
