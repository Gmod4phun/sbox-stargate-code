using System.Text.Json.Serialization;

public class ToggleableLight : Component, IUse, Component.ExecuteInEditor
{
	[Property]
	public Light LightComponent { get; set; }

	[Property]
	public Color LightColor { get; set; } = Color.White;

	[Property]
	public bool IsOn { get; private set; }

	[Property, JsonIgnore]
	public ModelRenderer ModelRenderer => Components.Get<ModelRenderer>(FindMode.EnabledInSelf);

	[Property]
	public string OnSkin { get; set; } = "";

	private float _lightIntensity;

	public void ToggleLight()
	{
		IsOn = !IsOn;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		_lightIntensity = _lightIntensity.LerpTo(IsOn ? 1 : 0, Time.Delta * (IsOn ? 5 : 25));

		if (!LightComponent.IsValid())
			return;

		LightComponent.LightColor = LightColor * _lightIntensity;

		if (!ModelRenderer.IsValid())
			return;

		ModelRenderer.SceneObject.Batchable = false;
		ModelRenderer.SceneObject.Attributes.Set("selfillumscale", _lightIntensity);
		ModelRenderer.SceneObject.Attributes.Set("colortint", IsOn ? LightColor : Color.White);

		if (!string.IsNullOrEmpty(OnSkin))
		{
			ModelRenderer.MaterialGroup = IsOn ? OnSkin : "default";
		}
	}

	public bool IsUsable(GameObject user)
	{
		return true;
	}

	public bool OnUse(GameObject user)
	{
		ToggleLight();
		return false;
	}
}
