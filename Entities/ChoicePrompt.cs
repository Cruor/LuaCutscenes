using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.LuaCutscenes {
    [Tracked]
    public class ChoicePrompt : Entity {
        internal static void Load() {
            On.Celeste.Level.SkipCutscene += ClearOptionsOnSkip;
        }

        internal static void Unload() {
            On.Celeste.Level.SkipCutscene -= ClearOptionsOnSkip;
        }
        
        private static void ClearOptionsOnSkip(On.Celeste.Level.orig_SkipCutscene orig, Level self) {
            orig(self);
            var toRemove = new List<Entity>(Engine.Scene.Tracker.GetEntities<ChoicePrompt>());
            foreach (var e in toRemove) {
                e.RemoveSelf();
            }
        }
        
        public static int Choice;
        
        public static IEnumerator Prompt(params string[] options) {
            var obj = new ChoicePrompt();
            Engine.Scene.Add(obj);
            foreach (var opt in options) {
                obj.Add(new Option(opt));
            }

            Audio.Play("event:/ui/game/chatoptions_appear");
            while (obj.Alive) {
                yield return null;
            }

            Choice = obj.Index;
            obj.RemoveSelf();
        }

        private List<Option> Options = new List<Option>();
        private bool Alive, Confirmed;
        private int Index;

        public ChoicePrompt() {
            this.Tag = (int) Tags.HUD;
            this.Alive = true;
        }

        public void Add(Option option) {
            int idx = this.Options.Count;
            this.Options.Add(option);
            Engine.Scene.Add(option);

            option.Position = new Vector2(260f, (float) (120.0 + 160.0 * idx));
            option.Ease = 0f;
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            foreach (var opt in this.Options) {
                opt.RemoveSelf();
            }
        }


        public override void Update() {
            base.Update();

            if (this.Confirmed) {
                this.Alive = false;
                foreach (var opt in this.Options) {
                    opt.Ease = Calc.Approach(opt.Ease, 0f, Engine.DeltaTime * 4);
                    if (opt.Ease != 0f) {
                        this.Alive = true;
                    }
                }
            } else {
                if (Input.MenuConfirm.Pressed) {
                    Audio.Play("event:/ui/game/chatoptions_select");
                    this.Confirmed = true;
                } else if (Input.MenuUp.Pressed && this.Index > 0) {
                    Audio.Play("event:/ui/game/chatoptions_roll_up");
                    this.Index--;
                } else if (Input.MenuDown.Pressed && this.Index < this.Options.Count - 1) {
                    Audio.Play("event:/ui/game/chatoptions_roll_down");
                    this.Index++;
                }

                var idx = 0;
                foreach (var opt in this.Options) {
                    opt.Ease = Calc.Approach(opt.Ease, 1f, Engine.DeltaTime * 4);
                    opt.Highlight = Calc.Approach(opt.Highlight, idx == this.Index ? 1f : 0f, Engine.DeltaTime * 4);
                    opt.Portrait?.Update();
                    idx++;
                }
            }
        }
    }

    public class Option : Entity {
        public float Ease;
        public float Highlight;
        
        public string Key;
        public string Textbox;
        public FancyText.Text Text;
        public Sprite Portrait;
        public Facings PortraitSide;
        public float PortraitSize;
        
        public Option(string key) {
            this.Key = key;
            this.Tag = (int) Tags.HUD;
            
            int maxLineWidth = 1828;
            this.Text = FancyText.Parse(Dialog.Get(this.Key), maxLineWidth, -1);
            this.Textbox = "textbox/madeline_ask";
            foreach (FancyText.Node node in this.Text.Nodes) {
                if (!(node is FancyText.Portrait portrait)) {
                    continue;
                }

                this.Portrait = GFX.PortraitsSpriteBank.Create(portrait.SpriteId);
                this.Portrait.Play(portrait.IdleAnimation);
                this.PortraitSide = (Facings) portrait.Side;
                this.Textbox = "textbox/" + portrait.Sprite + "_ask";

                XmlElement xml = GFX.PortraitsSpriteBank.SpriteData[portrait.SpriteId].Sources[0].XML;
                if (xml != null) {
                    string textboxFallback = "textbox/" + xml.Attr("textbox", portrait.Sprite) + "_ask";

                    this.PortraitSize = xml.AttrInt("size", 160);
                    this.Textbox = xml.Attr("ask_textbox", textboxFallback);
                }

                break;
            }

            if (!GFX.Portraits.Has(this.Textbox))
            {
                this.Textbox = "textbox/madeline_ask";
            }
        }

        public override void Render() {
            if (this.Scene is Level level && level.Paused) {
                return;
            }

            float introEase = Monocle.Ease.CubeOut(this.Ease);
            float highlightEase = Monocle.Ease.CubeInOut(this.Highlight);

            var position = this.Position;
            position.Y += (float) (-32.0 * (1.0 - introEase));
            position.X += highlightEase * 32f;

            Color color1 = Color.Lerp(Color.Gray, Color.White, highlightEase) * introEase;
            float alpha = MathHelper.Lerp(0.6f, 1f, highlightEase) * introEase;

            if (this.Textbox != null)
            {
                GFX.Portraits[this.Textbox]?.Draw(position, Vector2.Zero, color1);
            }

            Facings facings = this.PortraitSide;
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                facings = (Facings) (-(int) facings);
            }

            float num2 = 100f;
            
            if (this.Portrait != null)
            {
                this.Portrait.Scale = Vector2.One * (num2 / this.PortraitSize);
                if (facings == Facings.Right) {
                    this.Portrait.Position = position + new Vector2((float) (1380.0 - num2 * 0.5), 70f);
                    this.Portrait.Scale.X *= -1f;
                } else {
                    this.Portrait.Position = position + new Vector2((float) (20.0 + num2 * 0.5), 70f);
                }

                this.Portrait.Color = Color.White * (float) (0.5 + highlightEase * 0.5) * introEase;
                this.Portrait.Render();
            }

            float num3 = (float) ((140.0 - ActiveFont.LineHeight * 0.699999988079071) / 2.0);
            Vector2 position1 = new Vector2(0.0f, position.Y + 70f);
            Vector2 justify = new Vector2(0.0f, 0.5f);
            if (facings == Facings.Right) {
                justify.X = 1f;
                position1.X = (float) (position.X + 1400.0 - 20.0) - num3 - num2;
            } else {
                position1.X = position.X + 20f + num3 + num2;
            }

            this.Text.Draw(position1, justify, Vector2.One * 0.7f, alpha);
        }
    }
}
