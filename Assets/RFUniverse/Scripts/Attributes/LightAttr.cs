﻿using RFUniverse;
using RFUniverse.Attributes;
using Robotflow.RFUniverse.SideChannels;
using System;
using UnityEngine;

public class LightAttrData : BaseAttrData
{
    public LightData lightData;
    public LightAttrData() : base()
    {
        type = "Light";
    }
    public LightAttrData(BaseAttrData b) : base(b)
    {
        if (b is LightAttrData)
            lightData = (b as LightAttrData).lightData;
        type = "Light";
    }

    public override void SetAttrData(BaseAttr attr)
    {
        base.SetAttrData(attr);
        ((LightAttr)attr).SetLightData(lightData);
    }
}
[Serializable]
public class LightData
{
    public Color color = Color.white;
    public LightType type = LightType.Point;
    public LightShadows shadow = LightShadows.Hard;
    public float intensity = 1;
    public float range = 10;
    public float spotAngle = 30;
}

[RequireComponent(typeof(Light))]
public class LightAttr : BaseAttr
{
    GameObject lightView;
    private void Awake()
    {
        if (lightView != null) return;
        lightView = GameObject.Instantiate(Resources.Load<GameObject>("LightView"));
        lightView.transform.parent = transform;
        lightView.transform.localPosition = Vector3.zero;
        lightView.transform.localRotation = Quaternion.identity;
    }

    new protected Light light = null;
    public Light Light
    {
        get
        {
            if (light == null)
                light = GetComponent<Light>();
            return light;
        }
    }
    public LightType Type
    {
        get
        {
            return Light.type;
        }
        set
        {
            Light.type = value;
            lightView?.transform.Find("Point").gameObject.SetActive(false);
            lightView?.transform.Find("Spot").gameObject.SetActive(false);
            lightView?.transform.Find("Directional").gameObject.SetActive(false);
            switch (Light.type)
            {
                case LightType.Point:
                    lightView?.transform.Find("Point").gameObject.SetActive(true);
                    break;
                case LightType.Spot:
                    lightView?.transform.Find("Spot").gameObject.SetActive(true);
                    break;
                case LightType.Directional:
                    lightView?.transform.Find("Directional").gameObject.SetActive(true);
                    break;
            }
        }
    }

    private LightData lightData = new LightData();

    [EditAttr("Rigidbody", "RFUniverse.EditMode.LightAttrUI")]
    public LightData LightData
    {
        get
        {
            if (lightData == null)
                lightData = GetLightData();
            return lightData;
        }
        set
        {
            lightData = value;
        }
    }
    public LightData GetLightData()
    {
        LightData data = new LightData();
        data.color = Light.color;
        data.type = Type;
        data.shadow = Light.shadows;
        data.intensity = Light.intensity;
        data.range = Light.range;
        data.spotAngle = Light.spotAngle;
        return data;
    }
    public void SetLightData(LightData data)
    {
        Light.color = data.color;
        Type = data.type;
        Light.shadows = data.shadow;
        Light.intensity = data.intensity;
        Light.range = data.range;
        Light.spotAngle = data.spotAngle;
    }
    public override void Init()
    {
        base.Init();
        Light.cullingMask &= ~(1 << PlayerMain.Instance.axisLayer);
        Light.cullingMask &= ~(1 << PlayerMain.Instance.tempLayer);
        Type = Light.type;
    }
    public override BaseAttrData GetAttrData()
    {
        LightAttrData data = new LightAttrData(base.GetAttrData());
        data.lightData = GetLightData();
        return data;
    }
    public override void AnalysisMsg(IncomingMessage msg, string type)
    {
        switch (type)
        {
            case "SetColor":
                SetColor(msg);
                return;
            case "SetIntensity":
                SetIntensity(msg);
                return;
            case "SetRange":
                SetRange(msg);
                return;
            case "SetType":
                SetType(msg);
                return;
            case "SetShadow":
                SetShadow(msg);
                return;
            case "SetSpotAngle":
                SetSpotAngle(msg);
                return;
        }
        base.AnalysisMsg(msg, type);
    }

    private void SetSpotAngle(IncomingMessage msg)
    {
        Light.spotAngle = msg.ReadFloat32();
    }

    private void SetShadow(IncomingMessage msg)
    {
        Light.shadows = (LightShadows)msg.ReadInt32();
    }

    private void SetType(IncomingMessage msg)
    {
        Type = (LightType)msg.ReadInt32();
    }

    private void SetRange(IncomingMessage msg)
    {
        Light.range = msg.ReadFloat32();
    }

    private void SetIntensity(IncomingMessage msg)
    {
        Light.intensity = msg.ReadFloat32();
    }

    private void SetColor(IncomingMessage msg)
    {
        Light.color = new Color(msg.ReadFloat32(), msg.ReadFloat32(), msg.ReadFloat32(), 1);
    }


}
