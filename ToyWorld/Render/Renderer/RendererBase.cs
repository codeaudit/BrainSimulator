﻿using GoodAI.ToyWorld.Control;
using Render.RenderRequests;
using System;
using System.Diagnostics;
using Render.RenderObjects.Buffers;
using Render.RenderObjects.Effects;
using Render.RenderObjects.Geometries;
using Render.RenderObjects.Textures;
using VRage.Collections;
using World.ToyWorldCore;

namespace Render.Renderer
{
    public abstract class RendererBase : IDisposable
    {
        #region Fields

        private readonly IterableQueue<RenderRequest> m_renderRequestQueue = new IterableQueue<RenderRequest>();

        internal readonly GeometryManager GeometryManager = new GeometryManager();
        internal readonly EffectManager EffectManager = new EffectManager();
        internal readonly TextureManager TextureManager = new TextureManager();
        internal readonly RenderTargetManager RenderTargetManager = new RenderTargetManager();

        #endregion

        #region Genesis

        internal RendererBase()
        {
            StaticVboFactory.Init();
        }

        public virtual void Dispose()
        {
            // Dispose of RRs
            foreach (var renderRequest in m_renderRequestQueue)
                renderRequest.Dispose();

            m_renderRequestQueue.Clear();

            StaticVboFactory.Clear();
        }

        #endregion

        #region Virtual stuff

        public abstract int Width { get; }
        public abstract int Height { get; }

        public abstract void CreateWindow(string title, int width, int height);
        public abstract void CreateContext();
        public abstract void MakeContextCurrent();
        public abstract void MakeContextNotCurrent();

        public virtual void Init()
        {
            m_renderRequestQueue.Clear();
        }

        public virtual void ProcessRequests(ToyWorld world)
        {
            MakeContextCurrent();

            foreach (RenderRequest renderRequest in m_renderRequestQueue)
                renderRequest.OnPreDraw();

            foreach (RenderRequest renderRequest in m_renderRequestQueue)
            {
                Process(renderRequest, world);
                CheckError();
            }

            foreach (RenderRequest renderRequest in m_renderRequestQueue)
                renderRequest.OnPostDraw();
        }

        protected virtual void Process(RenderRequest request, ToyWorld world)
        {
            request.Draw(this, world);
        }

        [Conditional("DEBUG")]
        public virtual void CheckError()
        { }

        #endregion


        public void EnqueueRequest(IRenderRequest request)
        {
            m_renderRequestQueue.Enqueue((RenderRequestBase)request);
        }

        public void EnqueueRequest(IAvatarRenderRequest request)
        {
            m_renderRequestQueue.Enqueue((AvatarRRBase)request);
        }
    }
}
