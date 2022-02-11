// Text params -> script context.
// Any global pre-processing is allowed here.
formula.contextCreate = (in Bitmap input, in string param) =>
{
  if (string.IsNullOrEmpty(param))
    return null;

  Dictionary<string, string> p = Util.ParseKeyValueList(param);

  float freq = 500.0f;

  // freq=<float>
  if (Util.TryParse(p, "freq", ref freq))
    freq = Util.Clamp(freq, 1.0f, 100000.0f);

  Dictionary<string, object> sc = new Dictionary<string, object>();
  sc["freq"]    = freq;
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
    R = G = B = 0.0f;
  }
};
