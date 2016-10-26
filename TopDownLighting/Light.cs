using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLighting
{
    public class Light: IDisposable
    {
        public Light(GraphicsDevice graphicsDevice, int shadowMapSideLengthPixels)
        {
            shadowMap = new RenderTarget2D(graphicsDevice, shadowMapSideLengthPixels, shadowMapSideLengthPixels, true, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }

        public Matrix GetViewMatrix(Vector3? up = null)
        {
            // TODO: Problematic if the light's pointing straight up :P
            return Matrix.CreateLookAt(WorldPosition, WorldPosition + WorldDirection, up ?? Vector3.Up);
        }

        public Matrix GetProjectionMatrix(float near, float far)
        {
            return Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(SpotAngleDegrees), 1f, near, far);
        }

        public void Dispose()
        {
            if (shadowMap != null)
            {
                shadowMap.Dispose();
                shadowMap = null;
            }
        }

        public Vector3 WorldPosition
        {
            get;
            set;
        }

        public Vector3 WorldDirection
        {
            get;
            set;
        }

        public float SpotAngleDegrees
        {
            get;
            set;
        }

        public float SpotExponent
        {
            get;
            set;
        }

        public float ConstantAttenuation
        {
            get;
            set;
        }

        public float LinearAttenuation
        {
            get;
            set;
        }

        public float QuadraticAttenuation
        {
            get;
            set;
        }

        private RenderTarget2D shadowMap;
    }
}
