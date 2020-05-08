using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FormationUtils {

    public static Vector2Int GetPosition(int index, int count) {

        Vector2Int result = Vector2Int.zero;
        var offset = Vector2Int.up;
        
        switch (count) {
            
            case 0:
            case 1:
                result = new Vector2Int(0, 0);
                break;
            
            case 2:
                switch (index) {
                    case 0:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 1:
                        result = new Vector2Int(1, 0);
                        break;
                }
                break;
            
            case 3:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                }
                break;
            
            case 4:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                    case 3:
                        result = new Vector2Int(0, -1);
                        break;
                }
                break;

            case 5:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                    case 3:
                        result = new Vector2Int(-1, -1);
                        break;
                    case 4:
                        result = new Vector2Int(1, -1);
                        break;
                }
                break;

            case 6:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                    case 3:
                        result = new Vector2Int(0, -1);
                        break;
                    case 4:
                        result = new Vector2Int(-1, -1);
                        break;
                    case 5:
                        result = new Vector2Int(1, -1);
                        break;
                }
                break;

            case 7:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                    case 3:
                        result = new Vector2Int(0, -1);
                        break;
                    case 4:
                        result = new Vector2Int(-1, -1);
                        break;
                    case 5:
                        result = new Vector2Int(1, -1);
                        break;
                    case 6:
                        result = new Vector2Int(0, -2);
                        break;
                }
                break;

            case 8:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                    case 3:
                        result = new Vector2Int(0, -1);
                        break;
                    case 4:
                        result = new Vector2Int(-1, -1);
                        break;
                    case 5:
                        result = new Vector2Int(1, -1);
                        break;
                    case 6:
                        result = new Vector2Int(1, -2);
                        break;
                    case 7:
                        result = new Vector2Int(-1, -2);
                        break;
                }
                break;

            case 9:
                switch (index) {
                    case 0:
                        result = new Vector2Int(0, 0);
                        break;
                    case 1:
                        result = new Vector2Int(-1, 0);
                        break;
                    case 2:
                        result = new Vector2Int(1, 0);
                        break;
                    case 3:
                        result = new Vector2Int(0, -1);
                        break;
                    case 4:
                        result = new Vector2Int(-1, -1);
                        break;
                    case 5:
                        result = new Vector2Int(1, -1);
                        break;
                    case 6:
                        result = new Vector2Int(0, -2);
                        break;
                    case 7:
                        result = new Vector2Int(1, -2);
                        break;
                    case 8:
                        result = new Vector2Int(-1, -2);
                        break;
                }
                break;
            
            default:
                offset = Vector2Int.zero;
                result = ME.ECS.MathUtils.GetSpiralPointByIndex(Vector2Int.zero, index);
                break;

        }
        
        return result + offset;

    }

}
