using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	[CodeGenerator( CodeGeneratorFlags.Instance | CodeGeneratorFlags.WrapPropertySet, "__PropertyChanged" )]
	public class OnChangeAttribute : Attribute
	{
		public string CallbackName { get; set; }

		public OnChangeAttribute( string callbackName )
		{
			CallbackName = callbackName;
		}
	}

	public class PropertyChangeComponent : Component
	{
		[EditorBrowsable( EditorBrowsableState.Never )]
		protected void __PropertyChanged<T>( WrappedPropertySet<T> p )
		{
			var property = TypeLibrary.GetMemberByIdent( p.MemberIdent ) as PropertyDescription;
			var attribute = property.Attributes.OfType<OnChangeAttribute>().FirstOrDefault();
			var oldValue = property.GetValue( this );

			// Call the original setter.
			p.Setter( p.Value );

			// Our value changed.
			if ( !oldValue.Equals( p.Value ) )
			{
				property.TypeDescription.GetMethod( attribute.CallbackName ).Invoke( this, new[] { oldValue, p.Value } );
			}
		}
	}
}
