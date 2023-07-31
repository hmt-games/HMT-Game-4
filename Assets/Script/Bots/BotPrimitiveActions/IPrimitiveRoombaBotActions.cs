//Will store a list of all primitive actions
using UnityEngine;

public interface IPrimitiveRoombaBotActions
{
    public enum RoombaBotMode
    {
        Water,
        Fertilize,
        Harvest,        
    }
    

    void Water(GridNode gridNode);

    void Fertilize(GridNode gridNode);

    void Harvest(GridNode gridNode);

    void ChangeMode(RoombaBotMode roombaBotMode);

    void GotoLoc(Vector2 gridNodeCoordinates);

}




// ============ IGNORE. NOT RELATED TO VERTICAL FARMING (Just helping out a friend ~ Varun) ============

/*
 * String name
 * MeshVariantType targetVariantType
 * Texture texture
 * int materialSlot
 * float materialVal
 */



/*
 protected virtual void SetNewVisibility(float alpha, MeshVariantType targetVariantType = MeshVariantType.None) {
            if(objectVariantType == targetVariantType || MeshVariantType.Copy == targetVariantType) {
                base.SetVisibility(alpha);
                if (!HasVariant) return;
            }
            foreach (var component in variantComponents) {
                component.SetVisibility(alpha, targetVariantType);
            }
        }

        public override void SetMaterialTexture(string name, Texture texture, int materialSlot = 0, MeshVariantType targetVariantType = MeshVariantType.None) {
            if (objectVariantType == targetVariantType || MeshVariantType.Copy == targetVariantType) {
                base.SetMaterialTexture(name, texture, materialSlot, targetVariantType);
                if (!HasVariant) return;
            }
            foreach (var component in variantComponents) {
                component.SetMaterialTexture(name, texture, materialSlot, targetVariantType);
            }
        }

        public override void SetMaterialPropertyFloat(string name, float materialVal, int materialSlot = 0, MeshVariantType targetVariantType = MeshVariantType.None) {
            if (objectVariantType == targetVariantType || MeshVariantType.Copy == targetVariantType) {
                base.SetMaterialPropertyFloat(name, materialVal, materialSlot);
                if (!HasVariant) return;
            }
            foreach (var component in variantComponents) {
                component.SetMaterialPropertyFloat(name, materialVal, materialSlot, targetVariantType);
            }
        }
 */

