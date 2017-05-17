Shader "Masked/Mask" {
	SubShader{
		Tags{ "Queue" = "Geometry-10" }
		Lighting Off
		ZWrite On
		ColorMask 0
		Pass{}
	}
}