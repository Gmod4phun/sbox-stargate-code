namespace Sandbox.Components.Stargate
{
    public partial class DhdAtlantis : Dhd
    {
        protected override string ButtonSymbols => "ABCDEFGHIJKLMNOPQRST123456789UVW0XYZ";

        public DhdAtlantis()
        {
            Data = new( "peg", "dhd.atlantis.press", "dhd.press_dial" );
            DialIsLock = true;
        }

        public override void CreateButtons() // visible models of buttons that turn on/off and animate
        {
            // SYMBOL BUTTONS

            for ( var i = 0; i < ButtonSymbols.Length; i++ )
            {
                var modelName = $"models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_{i + 1}.vmdl";
                var actionName = ButtonSymbols[i].ToString();
                CreateSingleButton( modelName, actionName );
            }

            // CENTER DIAL BUTTON
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_37.vmdl", "DIAL" );

            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_1.vmdl", "@" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_2.vmdl", "*" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_3.vmdl", "IRIS" );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_4.vmdl", "_", true );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_5.vmdl", ".", true );
            CreateSingleButton( "models/sbox_stargate/dhd_atlantis/buttons/dhd_atlantis_button_extra_6.vmdl", ",", true );
        }
    }
}
