using System;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class LineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public LayerMask layer;
        public Material normalTexMat;
        public Material normalLineMat;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    }
    
    public Setting setting = new Setting();

    public class DrawNormalLinePass : ScriptableRenderPass
    {
        private Setting setting;
        LineFeature lineFeature;

        public DrawNormalLinePass(Setting setting, LineFeature lineFeature)
        {
            this.setting = setting;
            this.lineFeature = lineFeature;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID("_NormalLineTex"));
            cmd.ReleaseTemporaryRT(Shader.PropertyToID("_NormalTex"));
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("DrawNormalLine");
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            int normalTexID = Shader.PropertyToID("_NormalLineTex");
            cmd.GetTemporaryRT(normalTexID, descriptor);
            cmd.Blit(normalTexID, normalTexID, setting.normalLineMat, 0);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
    public class DrawNormalTexPass : ScriptableRenderPass
    {
        private Setting setting;
        ShaderTagId shaderTag = new ShaderTagId("DepthOnly");
        FilteringSettings filter;
        LineFeature feature;
        private RTHandle normalRTHandle;
        
        public DrawNormalTexPass(Setting setting, LineFeature feature)
        {
            this.setting = setting;
            this.feature = feature;
            
            RenderQueueRange renderQueueRange = new RenderQueueRange();
            renderQueueRange.lowerBound = 2000;
            renderQueueRange.upperBound = 3500;
            filter = new FilteringSettings(renderQueueRange, setting.layer);
        }
        
        
        [Obsolete("Configure方法已过时，请使用OnCameraSetup替代")]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // 空实现，所有配置逻辑移至OnCameraSetup
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            int temp = Shader.PropertyToID("_NormalTex");
            RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(temp, cameraTextureDescriptor);
            normalRTHandle = RTHandles.Alloc(temp); 
            // 配置渲染目标
            ConfigureTarget(normalRTHandle);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            // 释放RTHandle资源
            normalRTHandle?.Release();
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("DrawNormalTex");
            var drawingSettings = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawingSettings.overrideMaterial = setting.normalTexMat;
            drawingSettings.overrideMaterialPassIndex = 0;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filter);
            CommandBufferPool.Release(cmd);
        }
    }
    
    private DrawNormalTexPass _DrawNormalTexPass;
    private DrawNormalLinePass _DrawNormalLinePass;
    /// <inheritdoc/>
    public override void Create()
    {
        _DrawNormalTexPass = new DrawNormalTexPass(setting, this);
        _DrawNormalTexPass.renderPassEvent = setting.renderPassEvent;
        _DrawNormalLinePass = new DrawNormalLinePass(setting, this);
        _DrawNormalLinePass.renderPassEvent = setting.renderPassEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_DrawNormalTexPass);
        renderer.EnqueuePass(_DrawNormalLinePass);
    }
}