using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OrientPoint
{

        public Vector3 pos;
        public Quaternion rot;

        public OrientPoint(Vector3 pos, Quaternion rot)
        {
                this.pos = pos;
                this.rot = rot;
        }
        public OrientPoint(Vector3 pos, Vector3 forward)
        {
                this.pos = pos;
                this.rot = Quaternion.LookRotation(forward);
        }

        public Vector3 LocalToWorld( Vector3 localSpacePos ) =>  pos + rot * localSpacePos;         
        public Vector3 LocalToVec( Vector3 localSpacePos ) =>  rot * localSpacePos;
}
