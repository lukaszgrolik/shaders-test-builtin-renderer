static const float4 COLOR_WHITE = float4(1, 1, 1, 1);
static const float4 COLOR_BLACK = float4(0, 0, 0, 1);
static const float4 COLOR_RED = float4(1, 0, 0, 1);
static const float4 COLOR_GREEN = float4(0, 1, 0, 1);
static const float4 COLOR_BLUE = float4(0, 0, 1, 1);
static const float4 COLOR_YELLOW = float4(1, 1, 0, 1);

static const float PI = 3.14159265359f;
static const float TAU = 2 * PI;

/***

    roundPrec

***/

float roundPrec(float val, float prec)
{
    return round(val / prec) * prec;
}

float3 roundPrec(float3 val, float prec)
{
    return round(val / prec) * prec;
}

/***

    inversedLerp

***/

float inversedLerp(float a, float b, float v)
{
    if (a == b) return v > a;

    return (v - a) / (b - a);
}

/***

    remap

***/

float remap(float v, float inMin, float inMax, float outMin, float outMax)
{
    return lerp(outMin, outMax, inversedLerp(inMin, inMax, v));
}
