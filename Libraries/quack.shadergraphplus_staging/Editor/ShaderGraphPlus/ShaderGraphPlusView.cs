using Editor;
using Editor.ShaderGraph;
using NodeEditorPlus;
using ShaderGraphPlus.Nodes;
using static Sandbox.VertexLayout;

namespace ShaderGraphPlus;

public class ShaderGraphPlusView : GraphView
{
	private readonly MainWindow _window;
	private readonly UndoStack _undoStack;

	protected override string ClipboardIdent => "shadergraphplus";

	protected override string ViewCookie => _window?.AssetPath;

	private static bool? _cachedConnectionStyle;

	public static bool EnableGridAlignedWires
	{
		get => _cachedConnectionStyle ??= EditorCookie.Get( "shadergraphplus.gridwires", false );
		set => EditorCookie.Set( "shadergraphplus.gridwires", _cachedConnectionStyle = value );
	}

	private ConnectionStyle _oldConnectionStyle;

	public new ShaderGraphPlus Graph
	{
		get => (ShaderGraphPlus)base.Graph;
		set => base.Graph = value;
	}

	public Action OnNewParameterNodeCreated { get; set; }
	public Action OnConstantNodeConvertedToParameter { get; set; }

	public Action<BaseNodePlus> OnNodeRemoved { get; set; }

	private readonly Dictionary<string, NodeEditorPlus.INodeType> AvailableNodes = new( StringComparer.OrdinalIgnoreCase );
	private readonly Dictionary<string, IBlackboardParameterType> AvailableParameters = new( StringComparer.OrdinalIgnoreCase );

	public override ConnectionStyle ConnectionStyle => EnableGridAlignedWires
	? GridConnectionStyle.Instance
	: ConnectionStyle.Default;

	public ShaderGraphPlusView( Widget parent, MainWindow window ) : base( parent )
	{
		_window = window;
		_undoStack = window.UndoStack;

		OnSelectionChanged += SelectionChanged;
	}

	/*
	protected override Pixmap CreateBackgroundPixmap()
	{
		var cs = new ComputeShader( "core/ShaderGraphPlus/graphView_grid_cs.shader" );
		var texture = CreateTexture( "graphViewTex", 2048, 2048 );

		cs.Attributes.Set( "TextureSize", new Vector2( texture.Width, texture.Height ) );
		cs.Attributes.Set( "RWOutputTexture", texture );

		cs.Dispatch( texture.Width, texture.Height, 1 );

		var bitmap = texture.GetBitmap( 0 );

		return Pixmap.FromBitmap( bitmap );
	}


	public static Texture CreateTexture( string name, int width, int height, ImageFormat imageFormat = ImageFormat.RGBA8888 )
	{
		return Texture.Create( width, height )
		.WithName( name )
		.WithUAVBinding()
		.WithFormat( imageFormat )
		.Finish();
	}
	*/

	protected override INodeType RerouteNodeType { get; } = new ClassNodeType( EditorTypeLibrary.GetType<ReroutePlus>() );
	protected override INodeType CommentNodeType { get; } = new ClassNodeType( EditorTypeLibrary.GetType<CommentNode>() );

	public void AddNodeType<T>()
		where T : BaseNodePlus
	{
		AddNodeType( EditorTypeLibrary.GetType<T>() );
	}

	public void AddNodeType( TypeDescription type )
	{
		var nodeType = new ClassNodeType( type );

		AvailableNodes.TryAdd( nodeType.Identifier, nodeType );
	}

	public void AddNodeType( string subgraphPath )
	{
		var subgraphTxt = Editor.FileSystem.Content.ReadAllText( subgraphPath );
		var subgraph = new ShaderGraphPlus();
		subgraph.Deserialize( subgraphTxt );
		if ( !subgraph.AddToNodeLibrary ) return;
		var nodeType = new SubgraphNodeType( subgraphPath, EditorTypeLibrary.GetType<SubgraphNode>() );
		nodeType.SetDisplayInfo( subgraph );
		AvailableNodes.TryAdd( nodeType.Identifier, nodeType );
	}

	public NodeEditorPlus.INodeType FindNodeType( Type type )
	{
		return AvailableNodes.TryGetValue( type.FullName!, out var nodeType ) ? nodeType : null;
	}

	public void AddParameterType<T>() where T : BaseBlackboardParameter
	{
		AddParameterType( EditorTypeLibrary.GetType<T>() );
	}

	public void AddParameterType( TypeDescription type )
	{
		var parameterType = new ClassParameterType( type );

		AvailableParameters.TryAdd( parameterType.Identifier, parameterType );
	}

	private IEnumerable<IBlackboardParameterType> GetRelevantParameters()
	{
		return AvailableParameters.Values.Where( x =>
		{
			if ( x is ClassParameterType classParameterType )
			{
				var targetType = classParameterType.Type.TargetType;

				// Only show material parameters when not in a subgraph
				if ( Graph.IsSubgraph && targetType == typeof( BoolParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( IntParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( FloatParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Float2Parameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Float3Parameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Float4Parameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( ColorParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Texture2DParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( TextureCubeParameter ) ) return false;

				// Only show subgraph input parameters when in a subgraph
				if ( !Graph.IsSubgraph && targetType == typeof( BoolSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( IntSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( FloatSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float2SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float3SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float4SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( ColorSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float2x2SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float3x3SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Float4x4SubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( GradientSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( Texture2DSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( TextureCubeSubgraphInputParameter ) ) return false;
				if ( !Graph.IsSubgraph && targetType == typeof( SamplerStateSubgraphInputParameter ) ) return false;

				// Ignore these for now
				if ( Graph.IsSubgraph && targetType == typeof( ShaderFeatureBooleanParameter ) ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( ShaderFeatureEnumParameter ) ) return false;
			}
			return true;
		} );
	}

	protected override void OnOpenContextMenu( Menu menu, NodePlug targetPlug )
	{
		base.OnOpenContextMenu( menu, targetPlug );

		var selectedNodes = SelectedItems.OfType<NodeUI>().ToArray();
		if ( selectedNodes.Length > 1 && !selectedNodes.Any( x => x.Node is BaseResult ) )
		{
			menu.AddOption( "Create Custom Node...", "add_box", () =>
			{
				var fd = new FileDialog( null );
				fd.Title = "Create Shader Graph Function";
				fd.Directory = Project.Current.RootDirectory.FullName;
				fd.DefaultSuffix = $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}";
				fd.SelectFile( $"untitled.{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}" );
				fd.SetFindFile();
				fd.SetModeSave();
				fd.SetNameFilter( $"ShaderGraph Function (*.{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension})" );
				if ( !fd.Execute() ) return;

				CreateSubgraphFromSelection( fd.SelectedFile );
			} );
		}

		if ( selectedNodes.Length > 1 && selectedNodes.All( x => x.Node is IConstantNode && x.Node is not IConstantMatrixNode ) )
		{
			var convertOption = menu.AddOption( $"Convert {selectedNodes.Count()} Constant nodes to {(Graph.IsSubgraph ? "Subgraph Input nodes" : "Material Parameter nodes")}", "swap_horiz", () =>
			{
				using var undoScope = UndoScope( $"Convert {selectedNodes.Count()} Constant nodes to {(Graph.IsSubgraph ? "Subgraph Input nodes" : "Material Parameter nodes")}" );
				var lastNode = selectedNodes.First().Node as BaseNodePlus;
				foreach ( var node in selectedNodes )
				{
					var baseNode = node.Node as BaseNodePlus;
					var constantNode = baseNode as IConstantNode;

					Graph.RemoveNode( baseNode );

					var newName = $"{(Graph.IsSubgraph ? "SubgraphInput" : "MaterialParameter")}";
					var id = 0;
					while ( Graph.ContainsParameterWithName( $"{newName}{id}" ) )
					{
						id++;
					}

					lastNode = ConvertConstantNodeToParameter( constantNode, $"{newName}{id}", node.Position );
				}

				RebuildFromGraph();

				// Select the last node in the list.
				_window.OnNodeSelected( lastNode );
				SelectNode( lastNode );
			} );
		}

		if ( selectedNodes.Length == 1 )
		{
			var item = selectedNodes.FirstOrDefault();

			if ( item is null )
				return;

			if ( item.Node is BaseNodePlus node && ConCommands.NodeDebugInfo )
			{
				menu.AddSeparator();

				node.DebugInfo( menu );
			}

			if ( item.Node is BaseNodePlus baseNode && baseNode is IConstantNode constantNode )
			{
				string nodeTypeTitle = constantNode.GetType() switch
				{
					Type t when t == typeof( BoolConstantNode ) => "Bool",
					Type t when t == typeof( IntConstantNode ) => "Int",
					Type t when t == typeof( FloatConstantNode ) => "Float",
					Type t when t == typeof( Float2ConstantNode ) => "Float2",
					Type t when t == typeof( Float3ConstantNode ) => "Float3",
					Type t when t == typeof( Float4ConstantNode ) => "Float4",
					Type t when t == typeof( ColorConstantNode ) => "Color",
					Type t when t == typeof( Float2x2ConstantNode ) => "Float2x2",
					Type t when t == typeof( Float3x3ConstantNode ) => "Float3x3",
					Type t when t == typeof( Float4x4ConstantNode ) => "Float4x4",
					Type t when t == typeof( GradientConstantNode ) => "Gradient",
					_ => throw new NotImplementedException( $"Unknown type \"{constantNode.GetType()}\"" ),
				};

				if ( !Graph.IsSubgraph && baseNode is IConstantMatrixNode )
				{
					return;
				}

				if ( !Graph.IsSubgraph && baseNode is GradientConstantNode )
				{
					return;
				}

				var convertOption = menu.AddOption( $"Convert {baseNode.DisplayInfo.Name} node to {nodeTypeTitle} {(Graph.IsSubgraph ? "Subgraph Input node" : "Material Parameter node")}", "swap_horiz", () =>
				{
					Dialog.AskString( ( string parameterName ) =>
					{
						using var undoScope = UndoScope( $"Convert {baseNode.DisplayInfo.Name} node to {nodeTypeTitle} {(Graph.IsSubgraph ? "Subgraph Input node" : "Material Parameter node")}" );

						Graph.RemoveNode( baseNode );

						var newNode = ConvertConstantNodeToParameter( constantNode, parameterName, item.Node.Position );

						RebuildFromGraph();

						_window.OnNodeSelected( newNode );
						SelectNode( newNode );
					},
					$"Specify {(Graph.IsSubgraph ? "input" : "parameter")} name for the new {nodeTypeTitle} {(Graph.IsSubgraph ? "Subgraph Input node" : "Material Parameter node")}." );
				} );
			}
		}
	}

	private BaseNodePlus ConvertConstantNodeToParameter( IConstantNode constantNode, string parameterName, Vector2 nodePosition )
	{
		BaseNodePlus node = null;
		var isSubgraph = Graph.IsSubgraph;

		if ( !isSubgraph )
		{
			string nodeFullName = constantNode switch
			{
				BoolConstantNode => DisplayInfo.ForType( typeof( BoolParameterNode ) ).Fullname,
				IntConstantNode => DisplayInfo.ForType( typeof( IntParameterNode ) ).Fullname,
				FloatConstantNode => DisplayInfo.ForType( typeof( FloatParameterNode ) ).Fullname,
				Float2ConstantNode => DisplayInfo.ForType( typeof( Float2ParameterNode ) ).Fullname,
				Float3ConstantNode => DisplayInfo.ForType( typeof( Float3ParameterNode ) ).Fullname,
				Float4ConstantNode => DisplayInfo.ForType( typeof( Float4ParameterNode ) ).Fullname,
				ColorConstantNode => DisplayInfo.ForType( typeof( ColorParameterNode ) ).Fullname,
				_ => throw new NotImplementedException( $"Constant node {constantNode.GetType()} can not find an accompyaning parameter node." ),
			};

			if ( AvailableNodes.TryGetValue( nodeFullName, out var nodeType ) )
			{
				var parameterNodeType = new ConstantToParameterNodeType( ((ClassNodeType)nodeType).Type, constantNode, parameterName );

				node = CreateNewNode( parameterNodeType, nodePosition ).Node as BaseNodePlus;

				if ( parameterNodeType.BlackboardParameter != null )
				{
					Graph.AddParameter( parameterNodeType.BlackboardParameter );

					OnConstantNodeConvertedToParameter?.Invoke();
				}
			}

			if ( node != null )
			{
				return node;
			}

			throw new Exception( $"Unable to convert constant node \"{constantNode.GetType()}\" to material parameter." );
		}
		else
		{
			if ( AvailableNodes.TryGetValue( DisplayInfo.ForType( typeof( SubgraphInput ) ).Fullname, out var nodeType ) )
			{
				var parameterNodeType = new ConstantToParameterNodeType( ((ClassNodeType)nodeType).Type, constantNode, parameterName );

				node = CreateNewNode( parameterNodeType, nodePosition ).Node as BaseNodePlus;

				if ( parameterNodeType.BlackboardParameter != null )
				{
					Graph.AddParameter( parameterNodeType.BlackboardParameter );

					OnConstantNodeConvertedToParameter?.Invoke();
				}
			}

			if ( node != null )
			{
				return node;
			}

			throw new Exception( $"Unable to convert constant node \"{constantNode.GetType()}\" to subgraph input parameter." );
		}
	}

	// Stop texture2D parameters from being duplicated upon dropping into the graphView from the blackboard.
	private bool _preventTexture2DParamDupeDragDropHack = false;

	public override void OnDragLeave()
	{
		if ( HasDropped )
		{
			return;
		}

		if ( NodePreview.IsValid() )
		{
			if ( _preventTexture2DParamDupeDragDropHack && NodePreview.Node is Texture2DParameterNode texture2DParameterNode )
			{
				Graph.RemoveParameter( texture2DParameterNode.BlackboardParameterIdentifier );
				OnNewParameterNodeCreated?.Invoke();
				_preventTexture2DParamDupeDragDropHack = false;
			}
		}

		base.OnDragLeave();
	}

	public override void OnDragDrop( DragEvent ev )
	{
		if ( HasDropped )
		{
			return;
		}

		if ( _preventTexture2DParamDupeDragDropHack && NodePreview.Node is Texture2DParameterNode )
		{
			var node = NodePreview.Node as Texture2DParameterNode;

			base.OnDragDrop( ev );

			var texture2DParameter = new Texture2DParameter()
			{
				Identifier = Guid.NewGuid(),
				Value = node.UI
			};

			// Hack
			{
				Graph.SetParameterNodeLinkedBlackboardId( node.UI.Name, texture2DParameter.Identifier );
			}

			Graph.AddParameter( texture2DParameter );
			OnNewParameterNodeCreated?.Invoke();

			_preventTexture2DParamDupeDragDropHack = false;
		}
		else
		{
			base.OnDragDrop( ev );
		}
	}

	protected override NodeEditorPlus.INodeType NodeTypeFromDragEvent( DragEvent ev )
	{
		if ( ev.Data.Assets.FirstOrDefault() is { } asset )
		{
			if ( asset.IsInstalled )
			{
				if ( string.Equals( Path.GetExtension( asset.AssetPath ), $".{ShaderGraphPlusGlobals.SubgraphAssetTypeExtension}", StringComparison.OrdinalIgnoreCase ) )
				{
					return new SubgraphNodeType( asset.AssetPath, EditorTypeLibrary.GetType<SubgraphNode>() );
				}
				else
				{
					var realAsset = asset.GetAssetAsync().Result;
					if ( realAsset.AssetType == AssetType.ImageFile )
					{
						int id = 0;
						while ( Graph.ContainsParameterWithName( $"texture{id}" ) )
						{
							id++;
						}
						var parameterName = $"texture{id}";

						_preventTexture2DParamDupeDragDropHack = true;

						return new Texture2DParameterNodeType( EditorTypeLibrary.GetType<Texture2DParameterNode>(), parameterName, asset.AssetPath );
					}
				}
			}
		}

		if ( ev.Data.Object is BaseBlackboardParameter blackboardParameter )
		{
			var isSubgraph = Graph.IsSubgraph;

			if ( !isSubgraph )
			{
				string nodeFullName = blackboardParameter switch
				{
					BoolParameter => DisplayInfo.ForType( typeof( BoolParameterNode ) ).Fullname,
					IntParameter => DisplayInfo.ForType( typeof( IntParameterNode ) ).Fullname,
					FloatParameter => DisplayInfo.ForType( typeof( FloatParameterNode ) ).Fullname,
					Float2Parameter => DisplayInfo.ForType( typeof( Float2ParameterNode ) ).Fullname,
					Float3Parameter => DisplayInfo.ForType( typeof( Float3ParameterNode ) ).Fullname,
					Float4Parameter => DisplayInfo.ForType( typeof( Float4ParameterNode ) ).Fullname,
					ColorParameter => DisplayInfo.ForType( typeof( ColorParameterNode ) ).Fullname,
					Texture2DParameter => DisplayInfo.ForType( typeof( Texture2DParameterNode ) ).Fullname,
					TextureCubeParameter => DisplayInfo.ForType( typeof( TextureCubeParameterNode ) ).Fullname,
					ShaderFeatureBooleanParameter => DisplayInfo.ForType( typeof( BooleanFeatureSwitchNode ) ).Fullname,
					ShaderFeatureEnumParameter => DisplayInfo.ForType( typeof( EnumFeatureSwitchNode ) ).Fullname,
					_ => throw new NotImplementedException(),
				};

				if ( AvailableNodes.TryGetValue( nodeFullName, out var nodeType ) )
				{
					var parameterNodeType = new ParameterNodeTypeDragDrop( ((ClassNodeType)nodeType).Type, blackboardParameter );

					return parameterNodeType;
				}
			}
			else
			{
				if ( AvailableNodes.TryGetValue( DisplayInfo.ForType( typeof( SubgraphInput ) ).Fullname, out var nodeType ) )
				{
					var subgraphInputNodeType = new SubgraphInputNodeType( ((ClassNodeType)nodeType).Type, blackboardParameter );

					return subgraphInputNodeType;
				}
			}
		}

		return AvailableNodes.TryGetValue( ev.Data.Text, out var type )
			? type
			: null;
	}

	protected override IEnumerable<INodeType> GetRelevantNodes( NodeQuery query )
	{
		return AvailableNodes.Values.Filter( query ).Where( x =>
		{
			if ( x is ClassNodeType classNodeType )
			{
				var targetType = classNodeType.Type.TargetType;
				if ( classNodeType.Type.HasAttribute<InternalNodeAttribute>() ) return false;
				if ( Graph.IsSubgraph && targetType == typeof( Result ) ) return false;
				if ( targetType == typeof( SubgraphNode ) && classNodeType.DisplayInfo.Name == targetType.Name.ToTitleCase() ) return false;
			}
			return true;
		} );
	}

	private static bool TryGetHandleConfig( Type type, out Type matchingType, out NodeHandleConfig config )
	{
		if ( ShaderGraphPlusTheme.NodeHandleConfigs.TryGetValue( type, out config ) )
		{
			matchingType = type;
			return true;
		}

		matchingType = null;
		return false;
	}

	protected override NodeHandleConfig OnGetHandleConfig( Type type )
	{
		if ( TryGetHandleConfig( type, out var matchingType, out var config ) )
		{
			return config with { Name = type == matchingType ? config.Name : null };
		}

		return base.OnGetHandleConfig( type );
	}

	protected override void OnPopulateNodeMenuSpecialOptions( Menu menu, Vector2 clickPos, NodePlug targetPlug, string filter )
	{
		base.OnPopulateNodeMenuSpecialOptions( menu, clickPos, targetPlug, filter );
		var isSubgraph = Graph.IsSubgraph;

		if ( targetPlug == null )
		{
			var newParameterMenu = menu.AddMenu( $"Create {(isSubgraph ? "Subgraph Input" : "Parameter")}", "add" );

			foreach ( var classType in GetRelevantParameters().OrderBy( x => x.Type.GetAttribute<OrderAttribute>().Value ) )
			{
				var targetType = classType.Type.TargetType;
				if ( targetType == typeof( ShaderFeatureBooleanParameter ) || targetType == typeof( ShaderFeatureEnumParameter ) )
					continue;

				newParameterMenu.AddOption( classType.Type.Title, classType.Type.Icon, () =>
				{
					Dialog.AskString( ( string parameterName ) =>
					{
						CreateNewParameterNode( classType, parameterName, clickPos );
					},
					$"Specify a name for the {(isSubgraph ? "subgraph input" : "parameter")}" );
				} );
			}
		}

		if ( isSubgraph )
		{
			var newSubgraphOutputMenu = menu.AddMenu( $"Create Subgraph Output", "add" );

			newSubgraphOutputMenu.AddOption( "Bool", "check_box", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Bool );
			} );
			newSubgraphOutputMenu.AddOption( "Int", "looks_one", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Int );
			} );
			newSubgraphOutputMenu.AddOption( "Float", "looks_one", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Float );
			} );
			newSubgraphOutputMenu.AddOption( "Float2", "looks_two", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Vector2 );
			} );
			newSubgraphOutputMenu.AddOption( "Float3", "looks_3", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Vector3 );
			} );
			newSubgraphOutputMenu.AddOption( "Float4", "looks_4", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Vector4 );
			} );
			newSubgraphOutputMenu.AddOption( "Color", "palette", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Color );
			} );
			newSubgraphOutputMenu.AddOption( "Float2x2", "apps", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Float2x2 );
			} );
			newSubgraphOutputMenu.AddOption( "Float3x3", "apps", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Float3x3 );
			} );
			newSubgraphOutputMenu.AddOption( "Float4x4", "apps", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Float4x4 );
			} );
			newSubgraphOutputMenu.AddOption( "Gradient", "gradient", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Gradient );
			} );
			newSubgraphOutputMenu.AddOption( "Texture2D", "texture", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.Texture2DObject );
			} );
			newSubgraphOutputMenu.AddOption( "TextureCube", "view_in_ar", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.TextureCubeObject );
			} );
			newSubgraphOutputMenu.AddOption( "Sampler State", "colorize", () =>
			{
				CreateNewSubgraphOutputNode( clickPos, SubgraphPortType.SamplerState );
			} );
		}

		if ( !targetPlug.IsValid() )
		{
			//menu.AddOption( "Add Named Reroute Declaration", "route", () =>
			//{
			//	var nodeType = new NamedRerouteDeclarationNodeType( EditorTypeLibrary.GetType<NamedRerouteDeclarationNode>() );
			//
			//	CreateNewNode( nodeType, clickPos, targetPlug );
			//} );

			var namedRerouteDeclarations = Graph.Nodes.OfType<NamedRerouteDeclarationNode>();

			if ( namedRerouteDeclarations.Any() )
			{
				var optionsMenu = menu.AddMenu( "Named Reroutes", "route" );

				foreach ( var namedReroute in namedRerouteDeclarations )
				{
					optionsMenu.AddOption( namedReroute.Name, "route", () =>
					{
						CreateNewNamedReroute( namedReroute.Name, clickPos );
					} );
				}
			}
		}
		else if ( targetPlug is PlugIn )
		{
			var namedRerouteDeclarations = Graph.Nodes.OfType<NamedRerouteDeclarationNode>();

			if ( namedRerouteDeclarations.Any() )
			{
				var optionsMenu = menu.AddMenu( "Named Reroutes", "route" );

				foreach ( var namedRerouteDeclaration in namedRerouteDeclarations )
				{
					optionsMenu.AddOption( namedRerouteDeclaration.Name, "route", () =>
					{
						var nodeType = new NamedRerouteNodeType( EditorTypeLibrary.GetType<NamedRerouteNode>(), namedRerouteDeclaration.Name );

						CreateNewNode( nodeType, clickPos, targetPlug );
					} );
				}
			}
		}
		else if ( targetPlug is PlugOut )
		{
			menu.AddOption( "Add Named Reroute Declaration", "route", () =>
			{
				Dialog.AskString( ( string namedRerouteName ) =>
				{
					CreateNewNamedRerouteDeclaration( namedRerouteName, clickPos, targetPlug );
				},
				"Specify a Named Reroute name" );
			} );
		}

		menu.AddSeparator();
	}

	protected override void OnDoubleClickNodeSpecial( NodeUI node )
	{
		base.OnDoubleClickNodeSpecial( node );

		if ( node.Node is NamedRerouteNode namedRerouteNode )
		{
			var namedRerouteDeclaration = Graph.FindNamedRerouteDeclarationNode( namedRerouteNode.Name );

			if ( namedRerouteDeclaration != null )
			{
				CenterOn( namedRerouteDeclaration.Position );
				//SelectNode( namedRerouteDeclaration );
				//_window.SetPropertiesTarget( namedRerouteDeclaration );
			}
		}
	}

	private void CreateNewSubgraphOutputNode( Vector2 position, SubgraphPortType outputType )
	{
		Dialog.AskString( ( string outputName ) =>
		{
			var nodeFullName = DisplayInfo.ForType( typeof( SubgraphOutput ) ).Fullname;

			if ( AvailableNodes.TryGetValue( nodeFullName, out var nodeType ) )
			{
				var parameterNodeType = new SubgraphOutputNodeType( ((ClassNodeType)nodeType).Type, outputType, outputName );

				CreateNewNode( parameterNodeType, position );
			}
		},
		$"Specify a name for the subgraph output" );
	}

	private void CreateNewParameterNode( IBlackboardParameterType targetType, string name, Vector2 position )
	{
		var blackboardParameter = (BaseBlackboardParameter)targetType.CreateParameter( Graph );
		blackboardParameter.Name = name;

		var node = blackboardParameter.InitializeNode();
		node.Graph = Graph;
		node.Position = position.SnapToGrid( GridSize );

		Graph?.AddNode( node );

		OnNodeCreated( node );

		var nodeUI = node.CreateUI( this );

		Add( nodeUI );

		Graph.AddParameter( blackboardParameter );

		OnNewParameterNodeCreated?.Invoke();
	}

	private void CreateNewNamedReroute( string name, Vector2 position )
	{
		using var undoScope = UndoScope( "Add Named Reroute" );

		var nodeType = new NamedRerouteNodeType( EditorTypeLibrary.GetType<NamedRerouteNode>(), name );

		CreateNewNode( nodeType, position );
	}

	private void CreateNewNamedRerouteDeclaration( string name, Vector2 position, NodePlug targetPlug )
	{
		var nodeType = new NamedRerouteDeclarationNodeType( EditorTypeLibrary.GetType<NamedRerouteDeclarationNode>(), name );

		CreateNewNode( nodeType, position, targetPlug );
	}

	public override void ChildValuesChanged( Widget source )
	{
		BindSystem.Flush();

		base.ChildValuesChanged( source );

		BindSystem.Flush();
	}

	public override void PushUndo( string name )
	{
		Log.Info( $"Push Undo ({name})" );
		_undoStack.PushUndo( name, Graph.UndoStackSerialize() );
		_window.OnUndoPushed();
	}

	public override void PushRedo()
	{
		Log.Info( "Push Redo" );
		_undoStack.PushRedo( Graph.UndoStackSerialize() );
		_window.SetDirty();
	}

	private void CreateSubgraphFromSelection( string filePath )
	{
		if ( string.IsNullOrWhiteSpace( filePath ) ) return;

		var fileName = Path.GetFileNameWithoutExtension( filePath );
		var subgraph = new ShaderGraphPlus();
		subgraph.Title = fileName.ToTitleCase();
		subgraph.IsSubgraph = true;

		// Grab all selected nodes
		Vector2 rightmostPos = new Vector2( -9999, 0 );
		var selectedNodes = SelectedItems.OfType<NodeUI>();
		Dictionary<IPlugIn, IPlugOut> oldConnections = new();
		foreach ( var node in selectedNodes )
		{
			if ( node.Node is not BaseNodePlus baseNode ) continue;

			foreach ( var input in baseNode.Inputs )
			{
				oldConnections[input] = input.ConnectedOutput;
			}
			subgraph.AddNode( baseNode );

			rightmostPos.y += baseNode.Position.y;
			if ( baseNode.Position.x > rightmostPos.x )
			{
				rightmostPos = rightmostPos.WithX( baseNode.Position.x );
			}
		}
		rightmostPos.y /= selectedNodes.Count();

		// Create Inputs/Constants
		var nodesToAdd = new List<BaseNodePlus>();
		var previousOutputs = new Dictionary<string, IPlugOut>();
		foreach ( var node in subgraph.Nodes )
		{
			foreach ( var input in node.Inputs )
			{
				var correspondingOutput = oldConnections[input];
				var correspondingNode = subgraph.Nodes.FirstOrDefault( x => x.Identifier == correspondingOutput?.Node?.Identifier );
				if ( correspondingOutput is not null && correspondingNode is null )
				{
					var inputName = $"{input.Identifier}_{correspondingOutput?.Node?.Identifier}";
					var existingParameterNode = nodesToAdd.OfType<IParameterNode>().FirstOrDefault( x => x.Name == inputName );
					if ( input.ConnectedOutput is not null )
					{
						previousOutputs[inputName] = input.ConnectedOutput;
					}
					if ( existingParameterNode is not null )
					{
						input.ConnectedOutput = (existingParameterNode as BaseNodePlus).Outputs.FirstOrDefault();
						continue;
					}

					if ( input.Type == typeof( Texture2DObject ) )
					{
						var texture2DObjectNodeInput = FindNodeType( typeof( Texture2DObjectNode ) ).CreateNode( subgraph );
						texture2DObjectNodeInput.Position = node.Position - new Vector2( 240, 0 );
						if ( texture2DObjectNodeInput is Texture2DObjectNode texture2DObjectNode )
						{
							texture2DObjectNode.Name = inputName;
							input.ConnectedOutput = texture2DObjectNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( texture2DObjectNode );
						}
					}
					else if ( input.Type == typeof( TextureCubeObject ) )
					{
						var textureCubeObjectNodeInput = FindNodeType( typeof( TextureCubeObjectNode ) ).CreateNode( subgraph );
						textureCubeObjectNodeInput.Position = node.Position - new Vector2( 240, 0 );
						if ( textureCubeObjectNodeInput is TextureCubeObjectNode textureCubeObjectNode )
						{
							textureCubeObjectNode.Name = inputName;
							input.ConnectedOutput = textureCubeObjectNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( textureCubeObjectNode );
						}
					}
					else if ( input.Type == typeof( Sampler ) )
					{
						var samplerNodeInput = FindNodeType( typeof( SamplerNode ) ).CreateNode( subgraph );
						samplerNodeInput.Position = node.Position - new Vector2( 240, 0 );
						if ( samplerNodeInput is SamplerNode samplerNode )
						{
							samplerNode.Name = inputName;
							input.ConnectedOutput = samplerNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( samplerNode );
						}
					}
					else if ( input.Type == typeof( Float2x2 ) )
					{
						var float2x2Input = FindNodeType( typeof( Float2x2ConstantNode ) ).CreateNode( subgraph );
						float2x2Input.Position = node.Position - new Vector2( 240, 0 );
						if ( float2x2Input is Float2x2ConstantNode float2x2Node )
						{
							float2x2Node.Name = inputName;
							input.ConnectedOutput = float2x2Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( float2x2Node );
						}
					}
					else if ( input.Type == typeof( Float3x3 ) )
					{
						var float3x3Input = FindNodeType( typeof( Float3x3ConstantNode ) ).CreateNode( subgraph );
						float3x3Input.Position = node.Position - new Vector2( 240, 0 );
						if ( float3x3Input is Float3x3ConstantNode float3x3Node )
						{
							float3x3Node.Name = inputName;
							input.ConnectedOutput = float3x3Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( float3x3Node );
						}
					}
					else if ( input.Type == typeof( Float4x4 ) )
					{
						var float4x4Input = FindNodeType( typeof( Float4x4ConstantNode ) ).CreateNode( subgraph );
						float4x4Input.Position = node.Position - new Vector2( 240, 0 );
						if ( float4x4Input is Float4x4ConstantNode float4x4Node )
						{
							float4x4Node.Name = inputName;
							input.ConnectedOutput = float4x4Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( float4x4Node );
						}
					}
					else if ( input.Type == typeof( bool ) )
					{
						var boolInput = FindNodeType( typeof( BoolParameterNode ) ).CreateNode( subgraph );
						boolInput.Position = node.Position - new Vector2( 240, 0 );
						if ( boolInput is BoolParameterNode boolNode )
						{
							boolNode.Name = inputName;
							input.ConnectedOutput = boolNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( boolNode );
						}
					}
					else if ( input.Type == typeof( float ) )
					{
						var floatInput = FindNodeType( typeof( FloatParameterNode ) ).CreateNode( subgraph );
						floatInput.Position = node.Position - new Vector2( 240, 0 );
						if ( floatInput is FloatParameterNode floatNode )
						{
							floatNode.Name = inputName;
							input.ConnectedOutput = floatNode.Outputs.FirstOrDefault();
							nodesToAdd.Add( floatNode );
						}
					}
					else if ( input.Type == typeof( Vector2 ) )
					{
						var vector2Input = FindNodeType( typeof( Float2ParameterNode ) ).CreateNode( subgraph );
						vector2Input.Position = node.Position - new Vector2( 240, 0 );
						if ( vector2Input is Float2ParameterNode vector2Node )
						{
							vector2Node.Name = inputName;
							input.ConnectedOutput = vector2Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( vector2Node );
						}
					}
					else if ( input.Type == typeof( Vector3 ) )
					{
						var vector3Input = FindNodeType( typeof( Float3ParameterNode ) ).CreateNode( subgraph );
						vector3Input.Position = node.Position - new Vector2( 240, 0 );
						if ( vector3Input is Float3ParameterNode vector3Node )
						{
							vector3Node.Name = inputName;
							input.ConnectedOutput = vector3Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( vector3Node );
						}
					}
					else if ( input.Type == typeof( ColorParameterNode ) )
					{
						var vector4Input = FindNodeType( typeof( ColorParameterNode ) ).CreateNode( subgraph );
						vector4Input.Position = node.Position - new Vector2( 240, 0 );
						if ( vector4Input is ColorParameterNode vector4Node )
						{
							vector4Node.Name = inputName;
							input.ConnectedOutput = vector4Node.Outputs.FirstOrDefault();
							nodesToAdd.Add( vector4Node );
						}
					}
				}
			}
		}

		var subgraphOutputs = new List<SubgraphOutput>();
		foreach ( var node in subgraph.Nodes )
		{
			foreach ( var output in node.Outputs )
			{

				var iNode = FindNodeType( typeof( SubgraphOutput ) ).CreateNode( subgraph );

				if ( iNode is SubgraphOutput subgraphOutput )
				{
					subgraphOutput.Position = rightmostPos + new Vector2( 240, 0 );

					var correspondingNode = Graph.Nodes.FirstOrDefault( x => !subgraph.Nodes.Contains( x ) && x.Inputs.Any( x => x.ConnectedOutput == output ) );
					if ( correspondingNode is null ) continue;
					var inputName = $"{output.Identifier}_{output.Node.Identifier}";

					subgraphOutput.OutputName = inputName;
					subgraphOutput.SetSubgraphPortTypeFromType( output.Type );
					subgraphOutput.InitializeNode();

					var input = subgraphOutput.Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == inputName );
					input.ConnectedOutput = output;

					subgraphOutputs.Add( subgraphOutput );
					break;
				}
			}
		}

		//nodesToAdd.Add( subgraphOutput );
		nodesToAdd.AddRange( subgraphOutputs );

		// Add all the newly created nodes
		foreach ( var node in nodesToAdd )
		{
			subgraph.AddNode( node );
		}

		/*
		// Create Output/Result node
		var frNode = FindNodeType( typeof( FunctionResult ) ).CreateNode( subgraph );
		
		if ( frNode is FunctionResult resultNode )
		{
			resultNode.Position = rightmostPos + new Vector2( 240, 0 );
			resultNode.FunctionOutputs = new();
			foreach ( var node in subgraph.Nodes )
			{
				foreach ( var output in node.Outputs )
				{
					var correspondingNode = Graph.Nodes.FirstOrDefault( x => !subgraph.Nodes.Contains( x ) && x.Inputs.Any( x => x.ConnectedOutput == output ) );
					if ( correspondingNode is null ) continue;
					var inputName = $"{output.Identifier}_{output.Node.Identifier}";
					resultNode.FunctionOutputs.Add( new FunctionOutput
					{
						Name = inputName,
						TypeName = output.Type.FullName
					} );
					resultNode.CreateInputs();

					var input = resultNode.Inputs.FirstOrDefault( x => x is BasePlugIn plugIn && plugIn.Info.Name == inputName );
					input.ConnectedOutput = output;
					break;
				}
			}
			nodesToAdd.Add( resultNode );
		}

		// Add all the newly created nodes
		foreach ( var node in nodesToAdd )
		{
			subgraph.AddNode( node );
		}
		*/

		// Save the newly created sub-graph
		System.IO.File.WriteAllText( filePath, subgraph.Serialize() );
		var asset = AssetSystem.RegisterFile( filePath );
		MainAssetBrowser.Instance?.Local.UpdateAssetList();

		PushUndo( "Create Subgraph from Selection" );

		// Create the new subgraph node centered on the selected nodes
		Vector2 centerPos = Vector2.Zero;
		foreach ( var node in selectedNodes )
		{
			centerPos += node.Position;
		}
		centerPos /= selectedNodes.Count();
		var subgraphNode = CreateNewNode( new SubgraphNodeType( asset.RelativePath, EditorTypeLibrary.GetType<SubgraphNode>() ) ).Node as SubgraphNode;
		subgraphNode.Position = centerPos;

		// Get all the collected inputs/outputs and connect them to the new subgraph node
		foreach ( var node in Graph.Nodes )
		{
			if ( node == subgraphNode ) continue;

			if ( selectedNodes.Any( x => x.Node == node ) )
			{
				foreach ( var input in node.Inputs )
				{
					var correspondingOutput = oldConnections[input];
					if ( correspondingOutput is not null && !selectedNodes.Any( x => x.Node == correspondingOutput.Node ) )
					{
						var inputName = $"{input.Identifier}_{correspondingOutput.Node.Identifier}";
						var newInput = subgraphNode.Inputs.FirstOrDefault( x => x.Identifier == inputName );
						if ( previousOutputs.TryGetValue( inputName, out var previousOutput ) )
						{
							newInput.ConnectedOutput = previousOutput;
						}
					}
				}
			}
			else
			{
				foreach ( var input in node.Inputs )
				{
					var correspondingOutput = input.ConnectedOutput;
					if ( correspondingOutput is not null && selectedNodes.Any( x => x.Node == correspondingOutput.Node ) )
					{
						var inputName = $"{correspondingOutput.Identifier}_{correspondingOutput.Node.Identifier}";
						var newOutput = subgraphNode.Outputs.FirstOrDefault( x => x.Identifier == inputName );
						if ( newOutput is not null )
						{
							input.ConnectedOutput = newOutput;
						}
					}
				}
			}
		}

		PushRedo();
		DeleteSelection();

		// Delete all previously selected nodes
		UpdateConnections( Graph.Nodes );

	}

	private void SelectionChanged()
	{
		var item = SelectedItems
			.OfType<NodeUI>()
			.OrderByDescending( n => n is CommentUI )
			.FirstOrDefault();


		if ( !item.IsValid() )
		{
			_window.OnNodeSelected( null );
			return;
		}

		_window.OnNodeSelected( (BaseNodePlus)item.Node );
	}

	protected override void OnNodeCreated( IGraphNode node )
	{
		if ( node is SubgraphNode subgraphNode )
		{
			subgraphNode.OnNodeCreated();
		}
	}

	protected override void RemoveNode( NodeUI node )
	{
		base.RemoveNode( node );

		OnNodeRemoved?.Invoke( (BaseNodePlus)node.Node );
	}

	protected override void OnMouseClick( MouseEvent e )
	{
		base.OnMouseClick( e );

		var item = SelectedItems
			.OfType<NodeUI>()
			.OrderByDescending( n => n is CommentUI )
			.FirstOrDefault();

		if ( !item.IsValid() )
		{
			_window.OnGraphViewAreaClicked();
		}
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		foreach ( var node in Items )
		{
			if ( node is NodeUI nodeUI && nodeUI.Node is BaseNodePlus baseNode )
			{
				baseNode.OnFrame();
			}
		}

		if ( _oldConnectionStyle != ConnectionStyle )
		{
			_oldConnectionStyle = ConnectionStyle;

			foreach ( var connection in Items.OfType<NodeEditorPlus.Connection>() )
			{
				connection.Layout();
			}
		}
	}
}
