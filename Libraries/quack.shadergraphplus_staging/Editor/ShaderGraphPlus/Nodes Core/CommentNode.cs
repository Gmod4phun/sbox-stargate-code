using System.Text.Json.Nodes;

namespace ShaderGraphPlus;

[Icon( "notes" ), Hide]
public class CommentNode : BaseNodePlus, ICommentNode
{
	[Hide]
	public override int Version => 2;

	[Hide, Browsable( false )]
	public Vector2 Size { get; set; }

	public Color Color { get; set; } = Color.Parse( $"#33b679" )!.Value;

	public string Title { get; set; } = "Untitled";

	[TextArea]
	public string Description { get; set; } = "";

	[Sandbox.Range( 8, 64 )]
	public int DescriptionFontSize { get; set; } = 11;

	[Hide, Browsable( false )]
	public int Layer { get; set; } = 5;

	[Hide, JsonIgnore]
	public override DisplayInfo DisplayInfo
	{
		get
		{
			var info = DisplayInfo.For( this );

			info.Name = Title;
			info.Description = Description;

			return info;
		}
	}

	public override NodeUI CreateUI( GraphView view )
	{
		return new CommentUI( view, this );
	}

	#region Upgraders

	[SGPJsonUpgrader( typeof( CommentNode ), 2 )]
	public static void Upgrader_v2( JsonObject json )
	{
		if ( !json.ContainsKey( "Color" ) )
		{
			return;
		}

		try
		{
			var color = Color.Parse( $"#c2b5b5" )!.Value;

			switch ( json["Color"].ToString() )
			{
				case "White":
					color = Color.Parse( $"#c2b5b5" )!.Value;
					break;
				case "Red":
					color = Color.Parse( $"#d60000" )!.Value;
					break;
				case "Green":
					color = Color.Parse( $"#33b679" )!.Value;
					break;
				case "Blue":
					color = Color.Parse( $"#039be5" )!.Value;
					break;
				case "Yellow":
					color = Color.Parse( $"#f6c026" )!.Value;
					break;
				case "Purple":
					color = Color.Parse( $"#8e24aa" )!.Value;
					break;
				case "Orange":
					color = Color.Parse( $"#f5511d" )!.Value;
					break;
				default:
					color = Color.Parse( $"#c2b5b5" )!.Value;
					break;
			}

			json["Color"] = JsonSerializer.SerializeToNode( color, ShaderGraphPlus.SerializerOptions() );
		}
		catch
		{
		}
	}

	#endregion Upgraders
}
