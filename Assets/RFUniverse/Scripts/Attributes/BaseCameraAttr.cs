﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Robotflow.RFUniverse.SideChannels;
using System.Linq;
using UnityEngine.UIElements;


namespace RFUniverse.Attributes
{
    [RequireComponent(typeof(Camera))]
    public abstract class BaseCameraAttr : BaseAttr
    {
        new protected Camera camera = null;
        public Camera Camera
        {
            get
            {
                if (camera == null)
                    camera = GetComponent<Camera>();
                return camera;
            }
        }

        protected Texture2D tex = null;

        protected string rgbBase64String = null;
        protected string normalBase64String = null;
        protected string idBase64String = null;
        protected string depthBase64String = null;
        protected string depthEXRBase64String = null;
        protected string amodalMaskBase64String = null;
        protected string motionVectorBase64String = null;

        public override void Init()
        {
            base.Init();
            tex = new Texture2D(1, 1);

            Camera.enabled = false;
            Camera.depth = -100;
            Camera.allowMSAA = true;
            Camera.allowHDR = false;
            Camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
            Camera.cullingMask &= ~(1 << PlayerMain.Instance.axisLayer);
            Camera.cullingMask &= ~(1 << PlayerMain.Instance.tempLayer);
        }

        GameObject cameraView;
        private void Awake()
        {
            if (cameraView != null) return;
            cameraView = GameObject.Instantiate(Resources.Load<GameObject>("CameraView"));
            cameraView.transform.parent = transform;
            cameraView.transform.localPosition = Vector3.zero;
            cameraView.transform.localRotation = Quaternion.identity;
        }
        public override void CollectData(OutgoingMessage msg)
        {
            base.CollectData(msg);
            msg.WriteInt32(Camera.pixelWidth);
            msg.WriteInt32(Camera.pixelHeight);
            msg.WriteFloat32(Camera.fieldOfView);
            if (rgbBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(rgbBase64String);
                rgbBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (normalBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(normalBase64String);
                normalBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (idBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(idBase64String);
                idBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (depthBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(depthBase64String);
                depthBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (depthEXRBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(depthEXRBase64String);
                depthEXRBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (amodalMaskBase64String != null)
            {
                msg.WriteBoolean(true);
                msg.WriteString(amodalMaskBase64String);
                amodalMaskBase64String = null;
            }
            else
                msg.WriteBoolean(false);
            if (ddBBOX != null)
            {
                msg.WriteBoolean(true);
                msg.WriteInt32(ddBBOX.Count);
                foreach (var item in ddBBOX)
                {
                    msg.WriteInt32(item.Key);
                    msg.WriteFloat32(item.Value.x);
                    msg.WriteFloat32(item.Value.y);
                    msg.WriteFloat32(item.Value.width);
                    msg.WriteFloat32(item.Value.height);
                }
                ddBBOX = null;
            }
            else
                msg.WriteBoolean(false);
            if (dddBBOX != null)
            {
                msg.WriteBoolean(true);
                msg.WriteInt32(dddBBOX.Count);
                foreach (var item in dddBBOX)
                {
                    msg.WriteInt32(item.Key);
                    msg.WriteFloat32(item.Value.Item1.x);
                    msg.WriteFloat32(item.Value.Item1.y);
                    msg.WriteFloat32(item.Value.Item1.z);
                    msg.WriteFloat32(item.Value.Item2.x);
                    msg.WriteFloat32(item.Value.Item2.y);
                    msg.WriteFloat32(item.Value.Item2.z);
                    msg.WriteFloat32(item.Value.Item3.x);
                    msg.WriteFloat32(item.Value.Item3.y);
                    msg.WriteFloat32(item.Value.Item3.z);
                }
                dddBBOX = null;
            }
            else
                msg.WriteBoolean(false);
        }

        public override void AnalysisMsg(IncomingMessage msg, string type)
        {
            switch (type)
            {
                case "AlignView":
                    AlignView();
                    return;
                case "GetRGB":
                    GetRGB(msg);
                    return;
                case "GetNormal":
                    GetNormal(msg);
                    return;
                case "GetID":
                    GetID(msg);
                    return;
                case "GetDepth":
                    GetDepth(msg);
                    return;
                case "GetDepthEXR":
                    GetDepthEXR(msg);
                    return;
                case "GetAmodalMask":
                    GetAmodalMask(msg);
                    return;
                case "GetMotionVector":
                    GetMotionVector(msg);
                    return;
                case "Get2DBBox":
                    Get2DBBox(msg);
                    return;
                case "Get3DBBox":
                    Get3DBBox();
                    return;
            }
            base.AnalysisMsg(msg, type);
        }
        void AlignView()
        {
            transform.position = PlayerMain.Instance.MainCamera.transform.position;
            transform.rotation = PlayerMain.Instance.MainCamera.transform.rotation;
        }
        Texture2D GetRGB(IncomingMessage msg)
        {
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetRGB(intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetRGB(width, height, fov);
            }

        }
        public Texture2D GetRGB(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetRGB(size.x, size.y);
        }
        public abstract Texture2D GetRGB(int width, int height, float? fov = null);
        Texture2D GetNormal(IncomingMessage msg)
        {
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetNormal(intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetNormal(width, height, fov);
            }

        }
        public Texture2D GetNormal(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetNormal(size.x, size.y);
        }
        public abstract Texture2D GetNormal(int width, int height, float? unPhysicalFov = null);
        Texture2D GetID(IncomingMessage msg)
        {
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetID(intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetID(width, height, fov);
            }
        }
        public Texture2D GetID(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetID(size.x, size.y);
        }

        public abstract Texture2D GetID(int width, int height, float? unPhysicalFov = null);
        public Texture2D GetIDSingleChannel(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetIDSingleChannel(size.x, size.y);
        }
        public abstract Texture2D GetIDSingleChannel(int width, int height, float? unPhysicalFov = null);
        Texture2D GetDepth(IncomingMessage msg)
        {
            float near = msg.ReadFloat32();
            float far = msg.ReadFloat32();
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetDepth(intrinsicMatrix, near, far);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetDepth(width, height, near, far, fov);
            }
        }
        public Texture2D GetDepth(List<float> intrinsicMatrix, float near, float far)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetDepth(size.x, size.y, near, far);
        }
        public abstract Texture2D GetDepth(int width, int height, float near, float far, float? unPhysicalFov = null);
        Texture2D GetDepthEXR(IncomingMessage msg)
        {
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetDepthEXR(intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetDepthEXR(width, height, fov);
            }
        }
        public Texture2D GetDepthEXR(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetDepthEXR(size.x, size.y);
        }
        public abstract Texture2D GetDepthEXR(int width, int height, float? unPhysicalFov = null);
        Texture2D GetAmodalMask(IncomingMessage msg)
        {
            int id = msg.ReadInt32();
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetAmodalMask(id, intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetAmodalMask(id, width, height, fov);
            }
        }
        public Texture2D GetAmodalMask(int id, List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetAmodalMask(size.x, size.y, id);
        }
        public abstract Texture2D GetAmodalMask(int id, int width, int height, float? unPhysicalFov = null);

        Texture2D GetMotionVector(IncomingMessage msg)
        {
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                return GetMotionVector(intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                return GetMotionVector(width, height, fov);
            }
        }
        public Texture2D GetMotionVector(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            return GetMotionVector(size.x, size.y);
        }
        public abstract Texture2D GetMotionVector(int width, int height, float? unPhysicalFov = null);

        public Vector2Int SetCameraIntrinsicMatrix(Camera set_camera, List<float> intrinsicMatrix)
        {
            set_camera.usePhysicalProperties = true;
            float focal = 35;
            float ax, ay, sizeX, sizeY;
            float x0, y0, shiftX, shiftY;
            ax = intrinsicMatrix[0];
            ay = intrinsicMatrix[4];
            x0 = intrinsicMatrix[6];
            y0 = intrinsicMatrix[7];
            int width = (int)x0 * 2;
            int height = (int)y0 * 2;
            sizeX = focal * width / ax;
            sizeY = focal * height / ay;
            shiftX = -(x0 - width / 2.0f) / width;
            shiftY = (y0 - height / 2.0f) / height;
            set_camera.sensorSize = new Vector2(sizeX, sizeY);
            set_camera.focalLength = focal;
            set_camera.lensShift = new Vector2(shiftX, shiftY);
            return new Vector2Int(width, height);
        }

        protected List<int> SetTempLayer(BaseAttr target)
        {
            List<int> originLayers = new List<int>();
            foreach (var item in target.GetChildComponentFilter<Renderer>())
            {
                //if ((PlayerMain.Instance.simulationLayer.value & item.gameObject.layer) > 0)
                //{
                originLayers.Add(item.gameObject.layer);
                item.gameObject.layer = PlayerMain.Instance.tempLayer;
                //}
            }
            return originLayers;
        }
        protected void RevertLayer(BaseAttr target, List<int> originLayers)
        {
            List<Renderer> trans = target.GetChildComponentFilter<Renderer>();
            for (int i = 0; i < trans.Count; i++)
            {
                //if ((PlayerMain.Instance.simulationLayer.value & trans[i].gameObject.layer) > 0)
                trans[i].gameObject.layer = originLayers[i];
            }
        }


        Dictionary<int, Rect> ddBBOX = null;

        void Get2DBBox(IncomingMessage msg)
        {
            if (msg.ReadBoolean())
            {
                List<float> intrinsicMatrix = msg.ReadFloatList().ToList();
                Get2DBBox(intrinsicMatrix);
            }
            else
            {
                int width = msg.ReadInt32();
                int height = msg.ReadInt32();
                float fov = msg.ReadFloat32();
                Get2DBBox(width, height, fov);
            }
        }
        public void Get2DBBox(List<float> intrinsicMatrix)
        {
            Vector2Int size = SetCameraIntrinsicMatrix(Camera, intrinsicMatrix);
            Get2DBBox(size.x, size.y);
        }

        void Get2DBBox(int width, int height, float? unPhysicalFov = null)
        {
            Debug.Log("Get2DBBox");
            if (unPhysicalFov != null)
            {
                Camera.usePhysicalProperties = false;
                Camera.fieldOfView = unPhysicalFov.Value;
            }
            Camera.targetTexture = RenderTexture.GetTemporary(width, height);
            ddBBOX = new Dictionary<int, Rect>();
            foreach (var item in ActiveAttrs)
            {
                if (item.Value is BaseCameraAttr) continue;
                Rect rect = Get2DBBox(item.Value);
                if (rect.max.x > 0 && rect.max.y > 0 && rect.min.x < Camera.pixelWidth && rect.min.y < Camera.pixelHeight)
                    ddBBOX.Add(item.Key, rect);
            }
        }
        Rect Get2DBBox(BaseAttr attr)
        {
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            foreach (var render in attr.GetChildComponentFilter<MeshFilter>())
            {
                Vector3[] vertices = render.mesh.vertices;

                foreach (var item in vertices)
                {
                    Vector3 point = render.transform.TransformPoint(item);
                    point = Camera.WorldToScreenPoint(point);
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                    if (point.y > maxY) maxY = point.y;
                    if (point.y < minY) minY = point.y;
                }
            }
            return new Rect((maxX + minX) / 2, (maxY + minY) / 2, maxX - minX, maxY - minY);
        }
        Dictionary<int, Tuple<Vector3, Vector3, Vector3>> dddBBOX = null;
        void Get3DBBox()
        {
            dddBBOX = new Dictionary<int, Tuple<Vector3, Vector3, Vector3>>();
            foreach (var item in ActiveAttrs)
            {
                if (item.Value is BaseCameraAttr) continue;
                Tuple<Vector3, Vector3, Vector3> box = Get3DBBox(item.Value);
                Vector3 center = Camera.WorldToScreenPoint(box.Item1);
                if (center.x > 0 && center.y > 0 && center.x < Camera.pixelWidth && center.y < Camera.pixelHeight)
                    dddBBOX.Add(item.Key, box);
            }
        }
        Tuple<Vector3, Vector3, Vector3> Get3DBBox(BaseAttr attr)
        {
            float maxX = float.MinValue;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            float maxZ = float.MinValue;
            float minZ = float.MaxValue;
            foreach (var render in attr.GetChildComponentFilter<MeshFilter>())
            {
                Vector3[] vertices = render.mesh.vertices;

                foreach (var item in vertices)
                {
                    Vector3 point = render.transform.TransformPoint(item);
                    point = attr.transform.InverseTransformPoint(point);
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                    if (point.y > maxY) maxY = point.y;
                    if (point.y < minY) minY = point.y;
                    if (point.z > maxZ) maxZ = point.z;
                    if (point.z < minZ) minZ = point.z;
                }
            }
            Vector3 position = attr.transform.TransformPoint(new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2));
            Vector3 rotation = attr.transform.eulerAngles;
            Vector3 size = new Vector3((maxX - minX) * attr.transform.lossyScale.x, (maxY - minY) * attr.transform.lossyScale.y, (maxZ - minZ) * attr.transform.lossyScale.z);

            return new Tuple<Vector3, Vector3, Vector3>(position, rotation, size);
        }
    }
}
