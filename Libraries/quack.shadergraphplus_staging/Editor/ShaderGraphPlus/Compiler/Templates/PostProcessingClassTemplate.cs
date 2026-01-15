namespace ShaderGraphPlus;

public static class PostProcessingClassTemplate
{
	public static string Class => @"

using Sandbox;
using System;

[Title( ""{0}"" )]
[Category( ""{1}"" )]
[Icon( ""{2}"" )]
public sealed class {3} : Component, Component.ExecuteInEditor
{{
{4}

    IDisposable renderHook;

    protected override void OnEnabled()
    {{
        renderHook?.Dispose();
        var cc = Components.Get<CameraComponent>( true );
        renderHook = cc.AddHookBeforeOverlay( ""{3}"", {5}, RenderEffect );
    }}
	
    protected override void OnDisabled()
    {{
        renderHook?.Dispose();
        renderHook = null;
    }}

    RenderAttributes attributes = new RenderAttributes();

    public void RenderEffect( SceneCamera camera )
    {{
        if ( !camera.EnablePostProcessing )
            return;

		// Set Shader attributes.
{6}

		// Set Shader Combos.
		//attributes.SetCombo( ""D_DIRECTIONAL"", Directional );

		Graphics.GrabFrameTexture( ""ColorBuffer"", attributes );
        Graphics.GrabDepthTexture( ""DepthBuffer"", attributes );
        Graphics.Blit( Material.FromShader(""{7}""), attributes );
            
    }}
}}
";
}
