using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MathSupport;
using NCalc;
using OpenglSupport;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utilities;

namespace _113graph
{
  public class Graph
  {
    /// <summary>
    /// Form-data initialization.
    /// </summary>
    public static void InitParams (out string param, out string tooltip, out string expr, out string name, out MouseButtons trackballButton)
    {
      param           = "domain=[-5.0;5.0;-3.0;3.0],grid=[50;50]";
      tooltip         = "domain=[x_min,x_max,y_min,y_max],grid=[x_segments, z_segments]";
      expr            = "2.0*x-1.0*z*z+5.0";
      trackballButton = MouseButtons.Left;

      name            = "Lukáš Macek";
    }

    /// <summary>
    /// Cached expression.
    /// </summary>
    string expression = "";

    /// <summary>
    /// Cached param string.
    /// </summary>
    string param = "";

    //====================================================================
    // Scene data
    //====================================================================

    /// <summary>
    /// Current model's center point.
    /// </summary>
    public Vector3 center = Vector3.Zero;

    /// <summary>
    /// Current model's diameter.
    /// </summary>
    public float diameter = 4.0f;

    /// <summary>
    /// Near distance for the current model (must be positive).
    /// </summary>
    public float near = 0.1f;

    /// <summary>
    /// Far distance for the current model.
    /// </summary>
    public float far = 20.0f;

    /// <summary>
    /// Vertex array ([color] [normal] coord), index array
    /// </summary>
    uint[] VBOid = null;

    /// <summary>
    /// Currently allocated lengths of VBOs (in bytes)
    /// </summary>
    long[] VBOlen = null;

    /// <summary>
    /// Stride for vertex-array (in bytes).
    /// </summary>
    int stride = 0;

    /// <summary>
    /// Current texture.
    /// </summary>
    int texName = 0;

    /// <summary>
    /// Number of vertices (indices) to draw.. (default = draw only triangles)
    /// </summary>
    int numberOfTriangleIndeces = 0;

    /// <summary>
    /// Vertices have texture coordinates.
    /// </summary>
    bool haveTexture = false;

    /// <summary>
    /// Vertices have colors.
    /// </summary>
    bool haveColors = true;

    /// <summary>
    /// Vertices have normal vectors.
    /// </summary>
    bool haveNormals = false;

    public void InitSimulation (string par, string expr)
    {
      param = "";
      expression = "";
      RegenerateGraph(par, expr);
    }

    public void SetVertexColor(float value, bool NaN, bool infinity, out float R, out float G, out float B)
    {
      if (NaN)
      {
        R = 0f;
        G = 0f;
        B = 0f;
      }
      else if (infinity)
      {
        R = 1.0f;
        G = 1.0f;
        B = 1.0f;
      }
      else if (value < -5.0)
      {
        R = 0.49f;
        G = 0f;
        B = 1.0f;
      }
      else if (value < -3.0)
      {
        R = 0f;
        G = 0f;
        B = 1.0f;
      }
      else if (value < -1.0)
      {
        R = 0f;
        G = 0.49f;
        B = 1.0f;
      }
      else if (value < 0)
      {
        R = 0f;
        G = 1.0f;
        B = 1.0f;
      }
      else if (value < 1.0)
      {
        R = 0f;
        G = 1.0f;
        B = 0.49f;
      }
      else if (value < 3.0)
      {
        R = 0f;
        G = 1.0f;
        B = 0f;
      }
      else if (value < 5.0)
      {
        R = 0.49f;
        G = 1.0f;
        B = 0f;
      }
      else if (value < 8.0)
      {
        R = 1.0f;
        G = 1.0f;
        B = 0f;
      }
      else if (value < 10.0)
      {
        R = 1.0f;
        G = 0.49f;
        B = 0f;
      }
      else
      {
        R = 1.0f;
        G = 0f;
        B = 0f;
      }
    }

    /// <summary>
    /// Recompute the graph, prepare VBO content and upload it to the GPU...
    /// </summary>
    /// <param name="param">New param string.</param>
    /// <param name="expr">New expression string.</param>
    /// <returns>null if OK, error message otherwise.</returns>
    public string RegenerateGraph (string par, string expr)
    {
      if (expr == expression &&
          par == param)
        return null;                // nothing has changed => nothing to do..

      double xMin = -5.0, xMax = 5.0, zMin = -3.0, zMax = 3.0;
      uint xSegments = 50, zSegments = 50;

      //------------------------------------------------------------------
      // Input text params (form).

      Dictionary<string, string> p = Util.ParseKeyValueList(par);

      // System (rendering) params.
      Form1.form.UpdateRenderingParams(p);


      // My params:

      // domain: [xMin;xMax;zMin;zMax]
      List<double> dom = null;
      if (Util.TryParse(p, "domain", ref dom, ';') &&
          dom != null &&
          dom.Count >= 4)
      {
        xMin = dom[0];
        xMax = Math.Max(xMin + 1.0e-6, dom[1]);
        zMin = dom[2];
        zMax = Math.Max(zMin + 1.0e-6, dom[3]);
      }

      // grid: [xSegments;zSegments]
      List<int> grid = null;
      if (Util.TryParse(p, "grid", ref grid, ';') &&
         grid != null &&
         grid.Count >= 2)
      {
        xSegments = (uint)grid[0];
        zSegments = (uint)grid[1];
      }

      //------------------------------------------------------------------
      // Domain = grid xSegments times zSegments of rectangles:
      double x = xMin;
      double dx = (xMax - xMin) / xSegments;
      double z = zMin;
      double dz = (zMax - zMin) / zSegments;
      uint maxVertexIndexX = xSegments;
      uint maxVertexIndexZ = zSegments;
      uint numberOfVertices = (maxVertexIndexX + 1) * (maxVertexIndexZ + 1);
      numberOfTriangleIndeces = (int)(6 * maxVertexIndexX * maxVertexIndexZ); // pilot ... indices
      uint dRow = maxVertexIndexX + 1;
      float r, g, b;
      double nx, ny, nz;
      double x1, y1, z1, x2, y2, z2;
      const double epsilon = 10e-16;
      const int seed = 0;
      Random random = seed == 0 ? new Random() : new Random(seed);
      double dnx, dnz;
      double nNorm;

      Expression e = null;
      double y;

      Vector3 v = new Vector3();
      Vector3 n = new Vector3();

      // Checking expression syntax first (only once).
      try
      {
        e = new Expression(expr);
        e.Options |= EvaluateOptions.IgnoreCase;
        e.Parameters["x"] = 0.0;
        e.Parameters["y"] = 0.0;
        e.Parameters["z"] = 0.0;
        y = (double)e.Evaluate();
      }
      catch (Exception ex)
      {
        // Error message displayed on the form.
        return ex.Message;
      }

      //------------------------------------------------------------------
      // Data for VBO.

      // This has to be in sync with the actual buffer filling loop (see the unsafe block below)!
      haveTexture = false;
      haveColors = true;
      haveNormals = true;

      stride = Vector3.SizeInBytes;
      if (haveTexture)
        stride += Vector2.SizeInBytes;
      if (haveColors)
        stride += Vector3.SizeInBytes;
      if (haveNormals)
        stride += Vector3.SizeInBytes;

      long newVboSize = stride * numberOfVertices;     // pilot ... vertices
      long newIndexSize = sizeof(uint) * numberOfTriangleIndeces;

      // Vertex array: [texture:2D] [color:3D] [normal:3D] coordinate:3D
      GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
      if (newVboSize != VBOlen[0])
      {
        VBOlen[0] = newVboSize;
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)VBOlen[0], IntPtr.Zero, BufferUsageHint.DynamicDraw);
      }

      IntPtr videoMemoryPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
      bool nan = false;
      bool infinity = false;

      // The calculation of vertices:
      for (uint i = 0; i <= maxVertexIndexZ; i++)
      {
        for (uint j = 0; j <= maxVertexIndexX; j++)
        {
          // Example of regular expression evaluation (no need to use try-catch block here)
          e.Parameters["x"] = x;
          e.Parameters["y"] = z;
          e.Parameters["z"] = z;          
          
          if (Math.Abs(x) < 10e-15)
          {
            x = 0;
          }
          if (Math.Abs(z) < 10e-15)
          {
            z = 0;
          }

          y = (double)e.Evaluate();

          // Resetting extra information so it can be saved
          nan = false;
          infinity = false;

          if (double.IsNaN(y)) // e.g. 0/0 or asin(1.1)
          {
            nan = true;           
            y = 0.0; 
          }
          else if(double.IsInfinity(y) || Math.Abs(y) > 10e16)  // e.g. 1/0
          {
            infinity = true;
            y = 0.0;
          }

          // Approximate normal vectors:
          nx = 0.0;
          ny = 0.0;
          nz = 0.0;


          if (Math.Abs(y) >= 10e-16)  // result != 0.0
          {
            // Generating the second point:
            dnx = random.Next(1000, 10000) * epsilon;
            x1 = x + dnx;
            z1 = z;
            e.Parameters["x"] = x1;
            e.Parameters["y"] = z1;
            e.Parameters["z"] = z1;
            y1 = (double)e.Evaluate();

            // Calculating the first vector:
            x1 = x1 - x;
            y1 = y1 - y;
            z1 = z1 - z;

            // Generating the third point:
            dnz = random.Next(1000, int.MaxValue) * epsilon;
            x2 = x;
            z2 = z + dnz;
            e.Parameters["x"] = x2;
            e.Parameters["y"] = z2;
            e.Parameters["z"] = z2;
            y2 = (double)e.Evaluate();

            // Calculating the second vector:
            x2 = x2 - x;
            y2 = y2 - y;
            z2 = z2 - z;

            // Calculating the normal vector using cross product:
            nx = z1 * y2 - y1 * z2;
            ny = x1 * z2 - z1 * x2;
            nz = y1 * x1 - x1 * y2;
            // ... selecting the correct cross product (direction: +y).

            // Normalization:
            nNorm = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            nx = nx / nNorm;
            ny = ny / nNorm;
            nz = nz / nNorm;
          }

          // Everything seems to be OK.
          expression = expr;
          param = par;

          v = new Vector3((float)x, (float)y, (float)z);
          n = new Vector3((float)nx, (float)ny, (float)nz);
          

          unsafe
          {
            float* ptr = (float*)videoMemoryPtr.ToPointer();
            uint index = 9 * (dRow * i + j);

            SetVertexColor(v.Y, nan, infinity, out r, out g, out b);

            // Possible options:
            // [s t] [R G B] [N_x N_y N_z] x y z

            // Currently using:
            // [R G B] [N_x N_y N_z] x y z

            // Vertex[dRow * i + j]
            ptr[index]     = r;
            ptr[index + 1] = g;
            ptr[index + 2] = b;
            ptr[index + 3] = n.X;
            ptr[index + 4] = n.Y;
            ptr[index + 5] = n.Z;
            ptr[index + 6] = v.X;
            ptr[index + 7] = v.Y;
            ptr[index + 8] = v.Z;
          }
          x += dx;
        }

        x = xMin;
        z += dz;
      }
      GL.UnmapBuffer(BufferTarget.ArrayBuffer);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

      // Index buffer
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
      if (newIndexSize != VBOlen[1])
      {
        VBOlen[1] = newIndexSize;
        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)VBOlen[1], IntPtr.Zero, BufferUsageHint.StaticDraw);
      }

      videoMemoryPtr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
      unsafe
      {
        uint* ptr = (uint*)videoMemoryPtr.ToPointer();

        for (uint i = 0; i < maxVertexIndexZ; i++)
        {
          for (uint j = 0; j < maxVertexIndexX; j++)
          {
            // Triangle[2 * (i * dRow + j)]
            ptr[6 * (i * (dRow - 1) + j)] = i * dRow + j;
            ptr[6 * (i * (dRow - 1) + j) + 1] = (i + 1) * dRow + j + 1;
            ptr[6 * (i * (dRow - 1) + j) + 2] = i * dRow + j + 1;

            // Triangle[2 * (i * dRow + j) + 1]
            ptr[6 * (i * (dRow - 1) + j) + 3] = i * dRow + j;
            ptr[6 * (i * (dRow - 1) + j) + 4] = (i + 1) * dRow + j;
            ptr[6 * (i * (dRow - 1) + j) + 5] = (i + 1) * dRow + j + 1;
          }
        }
      }
      GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

      // !!! TODO: store any information about extension data here
      // !!! Example: M x N x 2 triangles, then 3 * K segments used for the axes...
      // !!! Rendering code 'RenderScene()' must reflect that!

      // Change the graph dimension.

      // New center of the scene (for the Trackball).
      center = new Vector3(v.X + 0.5f, 0.0f, v.Z + 0.5f);
      // New scene diameter (trying to keep current scaling).
      diameter =  4.0f;
      // Near and far will be used for perspective frustum.
      near     =  0.1f;
      far      = 20.0f;

      Form1.form.SetStatus($"Tri: {numberOfTriangleIndeces / 3}");

      return null;

      // !!!}}
    }

    //====================================================================
    // Rendering - OpenGL
    //====================================================================

    /// <summary>
    /// OpenGL init code (cold init).
    /// </summary>
    public void InitOpenGL (GLControl glc)
    {
      // Log OpenGL info just for curiosity.
      GlInfo.LogGLProperties();

      // General OpenGL.
      glc.VSync = true;
      GL.ClearColor(Color.FromArgb(14, 20, 40));    // darker "navy blue"
      GL.Enable(EnableCap.DepthTest);
      GL.Enable(EnableCap.VertexProgramPointSize);
      GL.ShadeModel(ShadingModel.Flat);

      // VBO init.
      VBOid = new uint[2];            // one big buffer for vertex data, another buffer for tri/line indices
      GL.GenBuffers(2, VBOid);
      if (GlInfo.LogError("VBO init"))
        Application.Exit();

      VBOlen = new long[2];           // current buffer lenghts in bytes

      // Shaders.
      InitShaderRepository();
      if (!SetupShaders())
      {
        Util.Log("Shader setup failed, giving up...");
        Application.Exit();
      }

      // Only shader VertexAttribPointers() or the very old glVertex() stuff.
      GL.DisableClientState(ArrayCap.VertexArray);
      GL.DisableClientState(ArrayCap.TextureCoordArray);
      GL.DisableClientState(ArrayCap.NormalArray);
      GL.DisableClientState(ArrayCap.ColorArray);

      // Texture.
      texName = GenerateTexture();
    }

    /// <summary>
    /// Generate static procedural texture.
    /// </summary>
    /// <returns>Texture handle or 0 if no texture will be used.</returns>
    int GenerateTexture ()
    {
      // !!! TODO: change this part if you want, return 0 if texture is not used.

      // Generated (procedural) texture.
      const int TEX_SIZE = 128;
      const int TEX_CHECKER_SIZE = 8;
      Vector3 colWhite = new Vector3(0.85f, 0.75f, 0.30f);
      Vector3 colBlack = new Vector3(0.15f, 0.15f, 0.60f);
      Vector3 colShade = new Vector3(0.15f, 0.15f, 0.15f);

      GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
      int texName = GL.GenTexture();
      GL.BindTexture(TextureTarget.Texture2D, texName);

      Vector3[] data = new Vector3[TEX_SIZE * TEX_SIZE];
      for (int y = 0; y < TEX_SIZE; y++)
        for (int x = 0; x < TEX_SIZE; x++)
        {
          int i = y * TEX_SIZE + x;
          bool odd = ((x / TEX_CHECKER_SIZE + y / TEX_CHECKER_SIZE) & 1) > 0;
          data[i] = odd ? colBlack : colWhite;

          // Add some fancy shading on the edges.
          if ((x % TEX_CHECKER_SIZE) == 0 || (y % TEX_CHECKER_SIZE) == 0)
            data[i] += colShade;
          if (((x + 1) % TEX_CHECKER_SIZE) == 0 || ((y + 1) % TEX_CHECKER_SIZE) == 0)
            data[i] -= colShade;

          // Add top-half texture markers.
          if (y < TEX_SIZE / 2)
          {
            if (x % TEX_CHECKER_SIZE == TEX_CHECKER_SIZE / 2 &&
                y % TEX_CHECKER_SIZE == TEX_CHECKER_SIZE / 2)
              data[i] -= colShade;
          }
        }

      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, TEX_SIZE, TEX_SIZE, 0, PixelFormat.Rgb, PixelType.Float, data);

      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);

      GlInfo.LogError("create-texture");

      texName = 0;  // No texture used.
      return texName;
    }

    /// <summary>
    /// De-allocated all the data associated with the given texture object.
    /// </summary>
    /// <param name="texName"></param>
    void DestroyTexture (ref int texName)
    {
      int tHandle = texName;
      texName = 0;
      if (tHandle != 0)
        GL.DeleteTexture(tHandle);
    }

    /// <summary>
    /// Application exit.
    /// </summary>
    public void Destroy ()
    {
      // Texture.
      DestroyTexture(ref texName);

      // Buffers.
      if (VBOid != null &&
          VBOid[0] != 0)
      {
        GL.DeleteBuffers(2, VBOid);
        VBOid = null;
      }

      // Shaders.
      DestroyShaders();
    }

    //====================================================================
    // Rendering - shaders
    //====================================================================

    /// <summary>
    /// Global GLSL program repository.
    /// </summary>
    Dictionary<string, GlProgramInfo> programs = new Dictionary<string, GlProgramInfo>();

    /// <summary>
    /// Current (active) GLSL program.
    /// </summary>
    GlProgram activeProgram = null;

    /// <summary>
    /// Fill the shader-repository.
    /// </summary>
    void InitShaderRepository ()
    {
      programs.Clear();
      GlProgramInfo pi;

      // Default program.
      pi = new GlProgramInfo("default", new GlShaderInfo[]
      {
        new GlShaderInfo(ShaderType.VertexShader,   "shaderVertex.glsl",   "113graph"),
        new GlShaderInfo(ShaderType.FragmentShader, "shaderFragment.glsl", "113graph")
      });
      programs[pi.name] = pi;

      // put more programs here:
      // pi = new GlProgramInfo( ..
      //   ..
      // programs[ pi.name ] = pi;
    }

    /// <summary>
    /// Init shaders registered in global repository 'programs'.
    /// </summary>
    /// <returns>True if succeeded.</returns>
    bool SetupShaders ()
    {
      activeProgram = null;

      foreach (var programInfo in programs.Values)
        if (programInfo.Setup())
          activeProgram = programInfo.program;

      if (activeProgram == null)
        return false;

      GlProgramInfo defInfo;
      if (programs.TryGetValue("default", out defInfo) &&
          defInfo.program != null)
        activeProgram = defInfo.program;

      return true;
    }

    void DestroyShaders ()
    {
      foreach (var prg in programs.Values)
        prg.Destroy();
    }

    //====================================================================
    // Rendering - graph
    //====================================================================

    /// <summary>
    /// Rendering code itself (separated for clarity).
    /// </summary>
    /// <param name="cam">Camera parameters.</param>
    /// <param name="style">Current rendering style.</param>
    /// <param name="primitiveCounter">Number of GL primitives rendered.</param>
    public void RenderScene (
      IDynamicCamera cam,
      RenderingStyle style,
      ref long primitiveCounter)
    {
      // Scene rendering.
      if (Form1.form.drawGraph &&
          VBOlen[0] > 0L &&
          activeProgram != null)    // buffers are nonempty & shaders are ready => render
      {
        // Vertex buffer: [texture] [color] [normal] coordinate
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);

        // GLSL shaders.
        activeProgram.EnableVertexAttribArrays();

        GL.UseProgram(activeProgram.Id);

        // Uniforms.
        Matrix4 modelView  = cam.ModelView;
        Matrix4 projection = cam.Projection;
        Vector3 eye        = cam.Eye;
        GL.UniformMatrix4(activeProgram.GetUniform("matrixModelView"),  false, ref modelView);
        GL.UniformMatrix4(activeProgram.GetUniform("matrixProjection"), false, ref projection);

        GL.Uniform3(activeProgram.GetUniform("globalAmbient"), ref style.globalAmbient);
        GL.Uniform3(activeProgram.GetUniform("lightColor"), ref style.whiteLight);
        GL.Uniform3(activeProgram.GetUniform("lightPosition"), ref style.lightPosition);
        GL.Uniform3(activeProgram.GetUniform("eyePosition"), ref eye);
        GL.Uniform3(activeProgram.GetUniform("Ka"), ref style.matAmbient);
        GL.Uniform3(activeProgram.GetUniform("Kd"), ref style.matDiffuse);
        GL.Uniform3(activeProgram.GetUniform("Ks"), ref style.matSpecular);
        GL.Uniform1(activeProgram.GetUniform("shininess"), style.matShininess);

        // Color handling.
        bool useGlobalColor = style.useGlobalColor;
        if (!haveColors)         // use global color if vertices haven't got color!
          useGlobalColor = true;
        GL.Uniform1(activeProgram.GetUniform("globalColor"), useGlobalColor ? 1 : 0);

        // Shading, color interpolation.
        // shadingGouraud <=> lighting is computed in VS
        // shadingPhong   <=> lighting is computed in FS (normal interpolation)
        // shadingSmooth  <=> colors are interpolated in FS (color interpolation)
        bool shadingGouraud = style.lighting && !style.phong;
        bool shadingPhong   = style.lighting && style.phong;
        bool shadingSmooth  = style.smooth;
        GL.Uniform1(activeProgram.GetUniform("shadingGouraud"), shadingGouraud ? 1 : 0);
        GL.Uniform1(activeProgram.GetUniform("shadingPhong"), shadingPhong ? 1 : 0);
        GL.Uniform1(activeProgram.GetUniform("shadingSmooth"), shadingSmooth ? 1 : 0);
        GL.Uniform1(activeProgram.GetUniform("useAmbient"), style.useAmbient ? 1 : 0);
        GL.Uniform1(activeProgram.GetUniform("useDiffuse"), style.useDiffuse ? 1 : 0);
        GL.Uniform1(activeProgram.GetUniform("useSpecular"), style.useSpecular ? 1 : 0);
        GlInfo.LogError("set-uniforms");

        // Color texture in slot #0.
        bool okTexture = style.texture && haveTexture;
        GL.Uniform1(activeProgram.GetUniform("useTexture"), okTexture ? 1 : 0);
        GL.Uniform1(activeProgram.GetUniform("texSurface"), 0);
        if (okTexture)
        {
          GL.ActiveTexture(TextureUnit.Texture0);
          GL.BindTexture(TextureTarget.Texture2D, texName);
        }
        GlInfo.LogError("set-texture");

        // Vertex attributes.
        IntPtr p = IntPtr.Zero;

        if (activeProgram.HasAttribute("texCoords"))
          GL.VertexAttribPointer(activeProgram.GetAttribute("texCoords"), 2, VertexAttribPointerType.Float, false, stride, p);
        if (haveTexture)
          p += Vector2.SizeInBytes;

        if (activeProgram.HasAttribute("color"))
          GL.VertexAttribPointer(activeProgram.GetAttribute("color"), 3, VertexAttribPointerType.Float, false, stride, p);
        if (haveColors)
          p += Vector3.SizeInBytes;

        if (activeProgram.HasAttribute("normal"))
          GL.VertexAttribPointer(activeProgram.GetAttribute("normal"), 3, VertexAttribPointerType.Float, true, stride, p);
        if (haveNormals)
          p += Vector3.SizeInBytes;

        GL.VertexAttribPointer(activeProgram.GetAttribute("position"), 3, VertexAttribPointerType.Float, false, stride, p);
        GlInfo.LogError("set-attrib-pointers");

        // Index buffer.
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

        // Draw!

        // !!!{{ CHANGE THIS PART if you want to add axes, legend, etc...

        // Triangle part of the scene.
        // Draw total 'numberOfTriangleIndeces' vertices from the beginning of the index-buffer,
        // that gives us 'numberOfTriangleIndeces / 3' triangles.
        GL.DrawElements(PrimitiveType.Triangles, numberOfTriangleIndeces, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GlInfo.LogError("draw-elements-shader");

        // How to draw lines (e.g. coordinate axes):
        // GL.DrawElements(PrimitiveType.Lines, lineVertices, DrawElementsType.UnsignedInt, lineOffset);
        // lineVertices ... number of vertex indices for lines (e.g. 'lineVertices/2' lines)
        // lineOffset   ... start offset in the index-buffer

        primitiveCounter += numberOfTriangleIndeces / 3;

        // !!!}}

        // Cleanup.
        GL.UseProgram(0);
        if (okTexture)
          GL.BindTexture(TextureTarget.Texture2D, 0);
      }
      else
      {
        // Color cube in very old OpenGL style!
        GL.Begin(PrimitiveType.Quads);

        GL.Color3(0.0f, 1.0f, 0.0f);          // Set The Color To Green
        GL.Vertex3(1.0f, 1.0f, -1.0f);        // Top Right Of The Quad (Top)
        GL.Vertex3(-1.0f, 1.0f, -1.0f);       // Top Left Of The Quad (Top)
        GL.Vertex3(-1.0f, 1.0f, 1.0f);        // Bottom Left Of The Quad (Top)
        GL.Vertex3(1.0f, 1.0f, 1.0f);         // Bottom Right Of The Quad (Top)

        GL.Color3(1.0f, 0.5f, 0.0f);          // Set The Color To Orange
        GL.Vertex3(1.0f, -1.0f, 1.0f);        // Top Right Of The Quad (Bottom)
        GL.Vertex3(-1.0f, -1.0f, 1.0f);       // Top Left Of The Quad (Bottom)
        GL.Vertex3(-1.0f, -1.0f, -1.0f);      // Bottom Left Of The Quad (Bottom)
        GL.Vertex3(1.0f, -1.0f, -1.0f);       // Bottom Right Of The Quad (Bottom)

        GL.Color3(1.0f, 0.0f, 0.0f);          // Set The Color To Red
        GL.Vertex3(1.0f, 1.0f, 1.0f);         // Top Right Of The Quad (Front)
        GL.Vertex3(-1.0f, 1.0f, 1.0f);        // Top Left Of The Quad (Front)
        GL.Vertex3(-1.0f, -1.0f, 1.0f);       // Bottom Left Of The Quad (Front)
        GL.Vertex3(1.0f, -1.0f, 1.0f);        // Bottom Right Of The Quad (Front)

        GL.Color3(1.0f, 1.0f, 0.0f);          // Set The Color To Yellow
        GL.Vertex3(1.0f, -1.0f, -1.0f);       // Bottom Left Of The Quad (Back)
        GL.Vertex3(-1.0f, -1.0f, -1.0f);      // Bottom Right Of The Quad (Back)
        GL.Vertex3(-1.0f, 1.0f, -1.0f);       // Top Right Of The Quad (Back)
        GL.Vertex3(1.0f, 1.0f, -1.0f);        // Top Left Of The Quad (Back)

        GL.Color3(0.0f, 0.0f, 1.0f);          // Set The Color To Blue
        GL.Vertex3(-1.0f, 1.0f, 1.0f);        // Top Right Of The Quad (Left)
        GL.Vertex3(-1.0f, 1.0f, -1.0f);       // Top Left Of The Quad (Left)
        GL.Vertex3(-1.0f, -1.0f, -1.0f);      // Bottom Left Of The Quad (Left)
        GL.Vertex3(-1.0f, -1.0f, 1.0f);       // Bottom Right Of The Quad (Left)

        GL.Color3(1.0f, 0.0f, 1.0f);          // Set The Color To Violet
        GL.Vertex3(1.0f, 1.0f, -1.0f);        // Top Right Of The Quad (Right)
        GL.Vertex3(1.0f, 1.0f, 1.0f);         // Top Left Of The Quad (Right)
        GL.Vertex3(1.0f, -1.0f, 1.0f);        // Bottom Left Of The Quad (Right)
        GL.Vertex3(1.0f, -1.0f, -1.0f);       // Bottom Right Of The Quad (Right)

        GL.End();

        primitiveCounter += 12;
      }
    }

    public void Intersect (ref Vector3d p0, ref Vector3d p1, ref double nearest)
    {
      // Compute intersection of the given ray (p0, p1) with the scene,
      // act upon that (i.e. set status string, ..)
      Vector2d uv;

#if false
      Vector3 A, B, C;
      double curr = Geometry.RayTriangleIntersection(ref p0, ref p1, ref A, ref B, ref C, out uv);
      if (!double.IsInfinity(curr) &&
          curr < nearest)
        nearest = curr;
#else
      Vector3d ul   = new Vector3d(-1.0, -1.0, -1.0);
      Vector3d size = new Vector3d( 2.0,  2.0,  2.0);
      if (Geometry.RayBoxIntersection(ref p0, ref p1, ref ul, ref size, out uv))
      {
        nearest = uv.X;
        Form1.form.SetStatus(string.Format(CultureInfo.InvariantCulture, "[{0:f2},{1:f2},{2:f2}]",
                                           p0.X + nearest * p1.X,
                                           p0.Y + nearest * p1.Y,
                                           p0.Z + nearest * p1.Z));
      }
#endif
    }

    //====================================================================
    // Interaction
    //====================================================================

    /// <summary>
    /// Handles mouse-button push.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool MouseButtonDown (MouseEventArgs e)
    {
      return false;
    }

    /// <summary>
    /// Handles mouse-button release.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool MouseButtonUp (MouseEventArgs e)
    {
      return false;
    }

    /// <summary>
    /// Handles mouse move.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool MousePointerMove (MouseEventArgs e)
    {
      return false;
    }

    /// <summary>
    /// Handles keyboard key press.
    /// </summary>
    /// <returns>True if handled.</returns>
    public bool KeyHandle (KeyEventArgs e)
    {
      return false;
    }
  }
}
