using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util.GameRepresentation;

namespace util.Propagate
{
    public static class Propagate
    {
        // primitive propagation function, internal use only
        private static Func<Vector2, Vector2> PUp =
            (Vector2 vec) => new Vector2(vec.x, ++vec.y);

        private static Func<Vector2, Vector2> PDown =
            (Vector2 vec) => new Vector2(vec.x, --vec.y);

        private static Func<Vector2, Vector2> PLeft =
            (Vector2 vec) => new Vector2(--vec.x, vec.y);

        private static Func<Vector2, Vector2> PRight =
            (Vector2 vec) => new Vector2(++vec.x, vec.y);

        private static Func<Vector2, Vector2> PTopRight =
            (Vector2 vec) => new Vector2(++vec.x, ++vec.y);

        private static Func<Vector2, Vector2> PTopLeft =
            (Vector2 vec) => new Vector2(--vec.x, ++vec.y);

        private static Func<Vector2, Vector2> PBottomLeft =
            (Vector2 vec) => new Vector2(--vec.x, --vec.y);

        private static Func<Vector2, Vector2> PBottomRight =
            (Vector2 vec) => new Vector2(++vec.x, --vec.y);

        private static Func<Vector2, Vector2> PSamePlace =
            (Vector2 vec) => new Vector2(vec.x, vec.y);


        // composite actions to be used when plants propagate
        // int in dict means offset for layers
        // e.g. the `List<Vector2>` on key `-1` propagate here at 1 layer lower
        public static Dictionary<int, List<Vector2>> PropagatePlant(PropagateType pType, Vector2 plantPos)
        {
            Dictionary<int, List<Vector2>> res = new Dictionary<int, List<Vector2>>();
            switch (pType)
            {
                case PropagateType.Cross:
                    List<Vector2> level0Cross = new List<Vector2>();
                    level0Cross.Add(PUp(plantPos));
                    level0Cross.Add(PDown(plantPos));
                    level0Cross.Add(PLeft(plantPos));
                    level0Cross.Add(PRight(plantPos));
                    res[0] = level0Cross;
                    break;
                case PropagateType.Square:
                    List<Vector2> level0Square = new List<Vector2>();
                    level0Square.Add(PUp(plantPos));
                    level0Square.Add(PDown(plantPos));
                    level0Square.Add(PLeft(plantPos));
                    level0Square.Add(PRight(plantPos));
                    level0Square.Add(PTopRight(plantPos));
                    level0Square.Add(PTopLeft(plantPos));
                    level0Square.Add(PBottomLeft(plantPos));
                    level0Square.Add(PBottomRight(plantPos));
                    res[0] = level0Square;
                    break;
                case PropagateType.VerticalDown:
                    List<Vector2> levelDown = new List<Vector2>();
                    levelDown.Add(PSamePlace(plantPos));
                    res[1] = levelDown;
                    break;
                default: // TODO: add all other p types later
                    res[0] = new List<Vector2>();
                    break;
            }

            return res;
        }
    }
}
