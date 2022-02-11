// Text params -> script context.
// Any global pre-processing is allowed here.
formula.contextCreate = (in Bitmap input, in string param) =>
{
  if (string.IsNullOrEmpty(param))
    return null;

  Dictionary<string, string> p = Util.ParseKeyValueList(param);
  Dictionary<string, object> sc = new Dictionary<string, object>();

  float freq = 500.0f;

  // freq=<float>
  if (Util.TryParse(p, "freq", ref freq))
    freq = Util.Clamp(freq, 1.0f, 100000.0f);
  sc["freq"] = freq;

  // bg=[R;G;B]
  float R = 0.0f, G = 0.0f, B = 0.0f;
  if (Util.TryParse(p, "bg", ref R, ref G, ref B, ';'))
    sc["bg"] = new float[] { R, G, B };

  sc["tooltip"] = "freq=<float> .. frequency for image generation (default=500)";

  return sc;
};

// Checkerboard.
formula.pixelCreate = (
  in ImageContext ic,
  out float R,
  out float G,
  out float B) =>
{
  double frequency = 500.0;
  double mul = frequency / ic.width;
  long ord = 0L;

  if (ic.y > 0)
  {
    double u = mul * (ic.x - ic.width / 2) / ic.y;
    double v = frequency / ic.y;
    ord = (long)(Math.Round(v) + Math.Round(-u));
  }

  // Output color.
  if ((ord & 1L) == 0)
  {
    R = G = B = 1.0f;
  }
  else
  {
    if (ic.context.TryGetValue("bg", out object bgo) &&
        bgo is float[] bg)
    {
      R = bg[0];
      G = bg[1];
      B = bg[2];
    }
    else
      R = G = B = 0.0f;
  }
};
