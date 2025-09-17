using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace CraftableCartography.MapComponents;

public class CCMapComponent : EntityMapComponent
{
    private MeshRef quadModel;
    private Vec2f viewPos = new Vec2f();
    private Matrixf mvMat = new Matrixf();
    private int color;
    
    public CCMapComponent(ICoreClientAPI capi, LoadedTexture texture, Entity entity, string color = null) : base(capi, texture, entity, color)
    {
        this.quadModel = capi.Render.UploadMesh(QuadMeshUtil.GetQuad());
        this.Texture = texture;
        this.entity = entity;
        this.color = color == null ? 0 : ColorUtil.Hex2Int(color) | -16777216;
    }

    public override void Render(GuiElementMap map, float dt)
    {
        IPlayer player = this.entity is EntityPlayer entity1 ? entity1.Player : (IPlayer) null;
        if (player != null && player.WorldData?.CurrentGameMode == EnumGameMode.Spectator || (this.entity is EntityPlayer entity2 ? (entity2.Controls.Sneak ? 1 : 0) : 0) != 0 && player != this.capi.World.Player)
            return;

        var roundedVec = this.entity.Pos.XYZ;
        roundedVec.X = RoundToNearest(roundedVec.X, 32);
        roundedVec.Z = RoundToNearest(roundedVec.Z, 32);
        
        map.TranslateWorldPosToViewPos(roundedVec, ref this.viewPos);
        float x = (float) map.Bounds.renderX + this.viewPos.X;
        float y = (float) map.Bounds.renderY + this.viewPos.Y;
        ICoreClientAPI api = map.Api;
        if (this.Texture.Disposed)
            throw new Exception("Fatal. Trying to render a disposed texture");
        if (this.quadModel.Disposed)
            throw new Exception("Fatal. Trying to render a disposed texture");
        this.capi.Render.GlToggleBlend(true);
        IShaderProgram engineShader = api.Render.GetEngineShader(EnumShaderProgram.Gui);
        if (this.color == 0)
        {
            engineShader.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
        }
        else
        {
            Vec4f outVal = new Vec4f();
            ColorUtil.ToRGBAVec4f(this.color, ref outVal);
            engineShader.Uniform("rgbaIn", outVal);
        }
        engineShader.Uniform("applyColor", 0);
        engineShader.Uniform("extraGlow", 0);
        engineShader.Uniform("noTexture", 0.0f);
        engineShader.BindTexture2D("tex2d", this.Texture.TextureId, 0);
        var entityYaw = this.entity.Pos.Yaw;
        this.mvMat
            .Set(api.Render.CurrentModelviewMatrix)
            .Translate(x, y, 60f)
            .Scale((float) this.Texture.Width, (float) this.Texture.Height, 0.0f)
            .Scale(0.5f, 0.5f, 0.0f)
            .RotateZ((float) (-(double) entityYaw + 3.1415927410125732));
        engineShader.UniformMatrix("projectionMatrix", api.Render.CurrentProjectionMatrix);
        engineShader.UniformMatrix("modelViewMatrix", this.mvMat.Values);
        api.Render.RenderMesh(this.quadModel);
    }
    
    private static double RoundToNearest(double value, double multiple)
    {
        return Math.Round(value / multiple) * multiple;
    }
}