﻿using System;
using System.Collections.Generic;
using HoDBlocks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace HoDBlocks.Keg
{
    public class BlockKeg : BlockLiquidContainerBase
    {
        private float kegCapacityLitres;
        private float ironHoopDropChance;
        private float kegTapDropChance;
        private bool kegDropWithLiquid;
        private long updateListenerId;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            LoadConfigValues();

            if (api.Side == EnumAppSide.Client)
            {
                RegisterConfigUpdateListener(api);
            }
        }

        private void LoadConfigValues()
        {
            kegCapacityLitres = HoDBlocksModSystem.LoadedConfig.KegCapacityLitres;
            ironHoopDropChance = HoDBlocksModSystem.LoadedConfig.KegIronHoopDropChance;
            kegTapDropChance = HoDBlocksModSystem.LoadedConfig.KegTapDropChance;
            kegDropWithLiquid = HoDBlocksModSystem.LoadedConfig.KegDropWithLiquid;
        }

        private void RegisterConfigUpdateListener(ICoreAPI api)
        {
            updateListenerId = api.Event.RegisterGameTickListener(dt =>
            {
                float newKegCapacity = HoDBlocksModSystem.LoadedConfig.KegCapacityLitres;
                float newIronHoopDropChance = HoDBlocksModSystem.LoadedConfig.KegIronHoopDropChance;
                float newKegTapDropChance = HoDBlocksModSystem.LoadedConfig.KegTapDropChance;
                bool newKegDropWithLiquid = HoDBlocksModSystem.LoadedConfig.KegDropWithLiquid;
                if (newKegCapacity != kegCapacityLitres || 
                    newIronHoopDropChance != ironHoopDropChance || 
                    newKegTapDropChance != kegTapDropChance ||
                    newKegDropWithLiquid != kegDropWithLiquid)
                {
                    LoadConfigValues();
                    api.Event.UnregisterGameTickListener(updateListenerId);
                }

            }, 5000);
        }
        public override float CapacityLitres => kegCapacityLitres;
        public override bool CanDrinkFrom => false;
        private float resistance = 100f;
        private float playNextSound = 0.7f;
        private bool choppingComplete = false;

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            return;
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (itemslot.Itemstack?.Block is BlockKeg)
            {
                handHandling = EnumHandHandling.PreventDefaultAction;
                return;
            }

            base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
        }

        protected override void tryEatBegin(ItemSlot slot, EntityAgent byEntity, ref EnumHandHandling handling, string eatSound = "drink", int eatSoundRepeats = 1)
        {
            if (slot.Itemstack?.Block is BlockKeg)
            {
                handling = EnumHandHandling.PreventDefaultAction;
                return;
            }

            base.tryEatBegin(slot, byEntity, ref handling, eatSound, eatSoundRepeats);
        }

        protected override bool tryEatStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, ItemStack spawnParticleStack = null)
        {
            if (slot.Itemstack?.Block is BlockKeg)
            {
                return false;
            }

            return base.tryEatStep(secondsUsed, slot, byEntity, spawnParticleStack);
        }

        protected override void tryEatStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity)
        {
            if (slot.Itemstack?.Block is BlockKeg)
            {
                return;
            }

            base.tryEatStop(secondsUsed, slot, byEntity);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            ItemSlot offHandSlot = byPlayer.Entity.LeftHandItemSlot;

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityKeg blockEntityKeg)
            {
                ItemStack heldItem = activeHotbarSlot?.Itemstack;
                ItemStack offHandItem = offHandSlot?.Itemstack;

                if (heldItem?.Collectible?.Tool == EnumTool.Axe)
                {
                    choppingComplete = false;
                    playNextSound = 0.7f;

                    StartAxeAnimation(byPlayer);
                    PlayChoppingSound(world, byPlayer, blockSel);
                    return true;
                }

                if (heldItem?.Collectible is ItemKegTap && offHandItem?.Collectible?.Code?.Path?.Contains("hammer") == true)
                {
                    choppingComplete = false;
                    playNextSound = 0.7f;
                    Block currentBlock = world.BlockAccessor.GetBlock(blockSel.Position);
                    if (currentBlock.Code.Path != "kegtapped")
                    {
                        StartTappingAnimation(byPlayer);
                        PlayTappingSound(world, byPlayer, blockSel);
                        return true;
                    }
                }

                return HandleLiquidInteraction(world, byPlayer, blockSel, blockEntityKeg, heldItem);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }


        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer,
            BlockSelection blockSel)
        {
            if (choppingComplete) return false;

            BlockEntityKeg kegEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityKeg;
            ItemStack heldItem = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack;
            if (heldItem?.Collectible?.Tool == EnumTool.Axe)
            {
                if (secondsUsed >= playNextSound)
                {
                    PlayChoppingSound(world, byPlayer, blockSel);
                    playNextSound += 0.65f;
                }

                if (secondsUsed >= (resistance / 50) - 0.1f)
                {
                    world.RegisterCallback((dt) =>
                    {
                        StopAxeAnimation(byPlayer);
                        world.BlockAccessor.BreakBlock(blockSel.Position, byPlayer);
                        DropKegItems(world, blockSel.Position, this.Code.Path == "kegtapped");
                        choppingComplete = true;
                    }, 500);

                    return false;
                }

                return true;
            }
            else if (heldItem?.Collectible is ItemKegTap &&
                     byPlayer.Entity.LeftHandItemSlot.Itemstack?.Collectible?.Code?.Path?.Contains("hammer") == true)
            {
                if (secondsUsed >= playNextSound)
                {
                    PlayTappingSound(world, byPlayer, blockSel);
                    playNextSound += 0.65f;
                }

                if (secondsUsed >= (resistance / 50) - 0.1f)
                {
                    world.RegisterCallback((dt) =>
                    {
                        StopTappingAnimation(byPlayer);
                        Block tappedKegBlock = world.GetBlock(new AssetLocation("hodblocks:kegtapped"));
                        ITreeAttribute tree = new TreeAttribute();
                        kegEntity.ToTreeAttributes(tree);
                        world.BlockAccessor.ExchangeBlock(tappedKegBlock.BlockId, blockSel.Position);

                        BlockEntity newBlockEntity = world.BlockAccessor.GetBlockEntity(blockSel.Position);
                        if (newBlockEntity is BlockEntityKeg newBlockEntityKeg)
                        {
                            newBlockEntityKeg.FromTreeAttributes(tree, world);
                            newBlockEntityKeg.MarkDirty(true);
                        }
                        ItemSlot activeHotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
                        if (activeHotbarSlot.Itemstack?.Collectible is ItemKegTap)
                        {
                            activeHotbarSlot.TakeOut(1);
                            activeHotbarSlot.MarkDirty();
                        }

                        choppingComplete = true;
                    }, 500);
                    return false;
                }

                return true;
            }

            return false;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!choppingComplete)
            {
                StopAxeAnimation(byPlayer);
                StopTappingAnimation(byPlayer);
            }
        }

        public override bool OnBlockInteractCancel(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, EnumItemUseCancelReason cancelReason)
        {
            if (!choppingComplete)
            {
                StopAxeAnimation(byPlayer);
                StopTappingAnimation(byPlayer);
            }

            return true;
        }

        private void DropKegItems(IWorldAccessor world, BlockPos position, bool isTappedKeg)
        {
            Random random = new Random();
            for (int i = 0; i < 2; i++)
            {
                if (random.NextDouble() < ironHoopDropChance)
                {
                    world.SpawnItemEntity(new ItemStack(world.GetItem(new AssetLocation("game:hoop-iron"))), position.ToVec3d());
                }
            }
            if (isTappedKeg && random.NextDouble() < kegTapDropChance)
            {
                world.SpawnItemEntity(new ItemStack(world.GetItem(new AssetLocation("hodblocks:kegtap"))), position.ToVec3d());
            }
        }

        private bool HandleLiquidInteraction(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel,
            BlockEntityKeg blockEntityKeg, ItemStack heldItem)
        {
            if (heldItem == null || heldItem.Collectible == null)
            {
                return false;
            }

            ILiquidInterface liquidItem = heldItem.Collectible as ILiquidInterface;
            if (liquidItem != null)
            {
                Block kegBlock = world.BlockAccessor.GetBlock(blockSel.Position);
                bool isTappedKeg = kegBlock.Code.Path == "kegtapped";

                if (liquidItem is ILiquidSource)
                {
                    ItemStack liquidInHand = liquidItem.GetContent(heldItem);
                    ItemStack contentInKeg = blockEntityKeg.GetContent();
                    WaterTightContainableProps contentProps =
                        BlockLiquidContainerBase.GetContainableProps(contentInKeg);
                    float currentLitres = contentInKeg != null && contentProps != null
                        ? (float)contentInKeg.StackSize / contentProps.ItemsPerLitre
                        : 0;
                    bool isEmpty = currentLitres <= 0;
                    if (liquidInHand != null && (contentInKeg == null ||
                                                 liquidInHand.Collectible.Equals(liquidInHand, contentInKeg,
                                                     GlobalConstants.IgnoredStackAttributes)))
                    {
                        return base.OnBlockInteractStart(world, byPlayer, blockSel);
                    }
                }

                if (liquidItem is ILiquidSink)
                {
                    ItemStack contentInKeg = blockEntityKeg.GetContent();
                    WaterTightContainableProps contentProps =
                        BlockLiquidContainerBase.GetContainableProps(contentInKeg);
                    float currentLitres = contentInKeg != null && contentProps != null
                        ? (float)contentInKeg.StackSize / contentProps.ItemsPerLitre
                        : 0;
                    bool isEmpty = currentLitres <= 0;
                    if (!isEmpty)
                    {
                        if (!isTappedKeg) 
                        {
                            return true;
                        }
                        else
                        { 
                            return base.OnBlockInteractStart(world, byPlayer, blockSel);
                        }
                    }
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool flag = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (flag && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityKeg blockEntityKeg)
            {
                float playerYaw = byPlayer.Entity.Pos.Yaw;
                float snapAngle = 0.3926991f;
                float snappedYaw = (float)(Math.Round(playerYaw / snapAngle) * snapAngle);
                blockEntityKeg.MeshAngle = snappedYaw;
                blockEntityKeg.MarkDirty(true, null);
                
                if (byItemStack.Attributes.HasAttribute("kegContent"))
                {
                    ItemStack kegContent = (byItemStack.Attributes["kegContent"] as ItemstackAttribute)?.value;
                    if (kegContent != null)
                    {
                        blockEntityKeg.SetContent(kegContent);
                    }
                }
            }

            return flag;
        }

        private void StartAxeAnimation(IPlayer byPlayer)
        {
            var entityAnimManager = byPlayer.Entity.AnimManager;

            if (!entityAnimManager.IsAnimationActive("axechop"))
            {
                entityAnimManager?.StartAnimation(new AnimationMetaData()
                {
                    Animation = "axeready",
                    Code = "axeready"
                });

                entityAnimManager?.StartAnimation(new AnimationMetaData()
                {
                    Animation = "axechop",
                    Code = "axechop",
                    BlendMode = EnumAnimationBlendMode.AddAverage,
                    AnimationSpeed = 1.65f,
                    HoldEyePosAfterEasein = 0.3f,
                    EaseInSpeed = 500f,
                    EaseOutSpeed = 500f,
                    Weight = 25f,
                    ElementWeight = new Dictionary<string, float>
                    {
                        { "UpperArmr", 20.0f },
                        { "LowerArmr", 20.0f },
                        { "UpperArml", 20.0f },
                        { "LowerArml", 20.0f },
                        { "UpperTorso", 20.0f },
                        { "ItemAnchor", 20.0f }
                    },
                    ElementBlendMode = new Dictionary<string, EnumAnimationBlendMode>
                    {
                        { "UpperArmr", EnumAnimationBlendMode.Add },
                        { "LowerArmr", EnumAnimationBlendMode.Add },
                        { "UpperArml", EnumAnimationBlendMode.Add },
                        { "LowerArml", EnumAnimationBlendMode.Add },
                        { "UpperTorso", EnumAnimationBlendMode.Add },
                        { "ItemAnchor", EnumAnimationBlendMode.Add }
                    }
                });
            }
        }

        private void StopAxeAnimation(IPlayer byPlayer)
        {
            var entityAnimManager = byPlayer.Entity.AnimManager;

            if (entityAnimManager?.IsAnimationActive("axechop") == true)
            {
                entityAnimManager?.StopAnimation("axechop");
                entityAnimManager?.ResetAnimation("axechop");
                StopAllPlayerAnimations(byPlayer);
            }

            entityAnimManager?.StartAnimation(new AnimationMetaData()
            {
                Animation = "idle1",
                Code = "idle",
                BlendMode = EnumAnimationBlendMode.Add,
            });
        }

        private void PlayChoppingSound(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            world.PlaySoundAt(new AssetLocation("game:sounds/block/chop2"), blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer, true, 32f, 1f);
        }

        private void StartTappingAnimation(IPlayer byPlayer)
        {
            var entityAnimManager = byPlayer.Entity.AnimManager;
            StopTappingAnimation(byPlayer);

            if (!entityAnimManager.IsAnimationActive("chiselready"))
            {
                entityAnimManager?.StartAnimation(new AnimationMetaData()
                {
                    Animation = "chiselready",
                    Code = "chiselready"
                });
            }
        }

        private void StopTappingAnimation(IPlayer byPlayer)
        {
            var entityAnimManager = byPlayer.Entity.AnimManager;
            if (entityAnimManager?.IsAnimationActive("chiselready") == true)
            {
                entityAnimManager?.StopAnimation("chiselready");
                entityAnimManager?.ResetAnimation("chiselready");
                StopAllPlayerAnimations(byPlayer);
            }

            entityAnimManager?.StartAnimation(new AnimationMetaData()
            {
                Animation = "idle1",
                Code = "idle",
                BlendMode = EnumAnimationBlendMode.Add,
            });
        }

        private void PlayTappingSound(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            world.PlaySoundAt(new AssetLocation("game:sounds/block/barrel"), blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer, true, 32f, 1f);
        }

        private void StopAllPlayerAnimations(IPlayer byPlayer)
        {
            var entityAnimManager = byPlayer.Entity.AnimManager;

            if (entityAnimManager != null && entityAnimManager.ActiveAnimationsByAnimCode != null)
            {
                foreach (var animCode in entityAnimManager.ActiveAnimationsByAnimCode.Keys)
                {
                    entityAnimManager.StopAnimation(animCode);
                }
            }
            entityAnimManager?.StartAnimation(new AnimationMetaData()
            {
                Animation = "idle1",
                Code = "idle",
                BlendMode = EnumAnimationBlendMode.Add,
            });
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            var blockEntityKeg = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityKeg;
            bool containsRot = blockEntityKeg?.GetContent()?.Collectible?.Code?.ToString() == "game:rot";

            if (kegDropWithLiquid && !containsRot)
            {
                base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
                return;
            }

            bool preventDefault = false;

            foreach (BlockBehavior blockBehavior in this.BlockBehaviors)
            {
                EnumHandling handled = EnumHandling.PassThrough;
                blockBehavior.OnBlockBroken(world, pos, byPlayer, ref handled);

                if (handled == EnumHandling.PreventDefault)
                {
                    preventDefault = true;
                }
                if (handled == EnumHandling.PreventSubsequent)
                {
                    return;
                }
            }

            if (preventDefault)
            {
                return;
            }

            if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
            {
                ItemStack kegStack = new ItemStack(this, 1);

                if (blockEntityKeg != null && !containsRot)
                {
                    var contentStack = blockEntityKeg.GetContent();
                    if (contentStack != null)
                    {
                        kegStack.Attributes.SetItemstack("kegContent", contentStack);
                    }
                }

                world.SpawnItemEntity(kegStack, pos.ToVec3d().Add(0.5, 0.5, 0.5));
                world.PlaySoundAt(this.Sounds.GetBreakSound(byPlayer), pos, 0.0, byPlayer, true, 32f, 1f);
            }

            if (this.EntityClass != null)
            {
                BlockEntity entity = world.BlockAccessor.GetBlockEntity(pos);
                entity?.OnBlockBroken(null);
            }

            world.BlockAccessor.SetBlock(0, pos);
        }
    }
}
