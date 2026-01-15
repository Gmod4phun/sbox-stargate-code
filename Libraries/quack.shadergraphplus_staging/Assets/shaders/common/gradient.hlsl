#ifndef COMMON_GRADIENT_H
#define COMMON_GRADIENT_H


#define MAX_COLOR_KEYS 8
#define MAX_ALPHA_KEYS 8

#define GRADIENT_MODE_LINEAR 0
#define GRADIENT_MODE_STEPPED 1

#define SAMPLE_STARTING_INDEX 1

class Gradient
{
    float4 colors[MAX_COLOR_KEYS]; 
    float time;
    float alphas[MAX_ALPHA_KEYS];
    int colorsLength;
    int alphasLength;
    int mode;

    static Gradient Init()
    {
        Gradient g;

		//g.mode = GRADIENT_MODE_STEPPED; // TODO : Expose gradient mode!!! 
		g.mode = GRADIENT_MODE_LINEAR;
		g.colorsLength = MAX_COLOR_KEYS;
		g.alphasLength = MAX_ALPHA_KEYS;
	
		//                    x     y     z     w
        g.colors[0]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[1]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[2]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[3]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[4]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[5]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[6]  = float4(0.00, 0.00, 0.00, 0.00);
		g.colors[7]  = float4(0.00, 0.00, 0.00, 0.00);
		
        g.alphas[0]  = 0.00;
		g.alphas[1]  = 0.00;
		g.alphas[2]  = 0.00;
		g.alphas[3]  = 0.00;
		g.alphas[4]  = 0.00;
		g.alphas[5]  = 0.00;
		g.alphas[6]  = 0.00;
		g.alphas[7]  = 0.00;
		

        return g;
    }

	static float4 SampleGradient( Gradient gradient, float Time )
	{
		float3 color = gradient.colors[0].rgb;

    	[unroll]
    	for ( int c = SAMPLE_STARTING_INDEX; c < gradient.colorsLength; c++ )
    	{
			float colorPos = saturate( ( Time - gradient.colors[ c  - SAMPLE_STARTING_INDEX].w ) / ( gradient.colors[c].w - gradient.colors[ c  - SAMPLE_STARTING_INDEX].w) ) * step( c, gradient.colorsLength - SAMPLE_STARTING_INDEX );
    	    
			color = lerp( color, gradient.colors[c].rgb, lerp( colorPos, step( 0.01, colorPos ), gradient.mode ) );
    	}

	    float alpha = gradient.alphas[0].x;
		
	    [unroll]
	    for (int a = SAMPLE_STARTING_INDEX; a < gradient.alphasLength; a++)
	    {
			// Old
			//float alphaPos = saturate( ( Time - gradient.alphas[ a - 1 ].y ) / ( gradient.alphas[a].y - gradient.alphas[ a - 1 ].y ) ) * step( a, gradient.alphasLength - 1 );

	        float alphaPos = saturate( ( Time - gradient.alphas[ a - 1 ] ) / ( gradient.alphas[a] - gradient.alphas[ a - 1 ] ) ) * step( a, gradient.alphasLength - 1 );
	        
			// Old
			//alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.mode));

			alpha = lerp(alpha, gradient.alphas[a], lerp(alphaPos, step(0.01, alphaPos), gradient.mode));
	    }

	    return float4( color, alpha );
	}
};

#endif // COMMON_GRADIENT_H