using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
using EmsPlus.UI.Custom.InspectMenu;
using Rage;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.UI.Helpers
{
    public class InspectionRenderer
    {
        private InspectionInput _input;

        public void Render(Rage.Graphics g, InspectionState state, BodyPartManager bodyParts, DiagnosticManager diagnostics, PatientDataManager patientData, InspectionInput input, List<InspectionAction> panelActions)
        {
            _input = input;

            _input.PanelActionButtons.Clear();
            _input.DiagnosticActionButtons.Clear();

            DrawHeader(state, input);
            DrawBodyParts(state, bodyParts);

            if (state.PanelSlide > 0.01f && bodyParts.SelectedPart != null)
                DrawDetailPanel(state, bodyParts, input, panelActions);

            if (state.DiagnosticSlide > 0.01f)
                DrawDiagnosticPanel(state, diagnostics);

            if (state.DataSlide > 0.01f) DrawDataPanel(state, patientData);

            DrawCursor(input, state, bodyParts.HoveredPart != null);
        }

        private void DrawHeader(InspectionState state, InspectionInput input)
        {
            var ba = state.BaseAlpha;
            var ta = state.TextAlpha;

            NativeUITools.DrawNativeRect(0, 0, Game.Resolution.Width, 140, Color.FromArgb(ba, 10, 15, 25));
            for (int i = 0; i < 3; i++)
                NativeUITools.DrawNativeRect(0, 136 + i, Game.Resolution.Width, 1, Color.FromArgb(MathHelper.Clamp((80 - i * 25), 0, 255), 0, 180, 255));

            NativeUITools.DrawNativeText(Localization.Get("TITLE_INSPECTION", "PATIENT INSPECTION"), 42, 42, 0.7f, Color.FromArgb(ta / 3, 0, 0, 0));
            NativeUITools.DrawNativeText(Localization.Get("TITLE_INSPECTION", "PATIENT INSPECTION"), 40, 40, 0.7f, Color.FromArgb(ta, 255, 255, 255));

            NativeUITools.DrawNativeText("●", 40, 78, 0.35f, Color.FromArgb(ta, 0, 255, 150));
            NativeUITools.DrawNativeText(Localization.Get("SUBTITLE_INSPECTION", "Select area to inspect"), 58, 76, 0.35f, Color.FromArgb(MathHelper.Clamp(ta - 50, 0, 255), 180, 200, 220));
            float btnW = 160f, btnH = 36f, btnY = 45f;
            input.DiagnosticsButton = new RectangleF(Game.Resolution.Width - 400, btnY, btnW, btnH);
            DrawButton(input.DiagnosticsButton, Localization.Get("BTN_DIAGNOSTICS", "[TAB] Diagnostics"), input.IsHovering(input.DiagnosticsButton), Color.FromArgb(255, 255, 180, 0), ba, ta);

            input.DataButton = new RectangleF(Game.Resolution.Width - 580, btnY, btnW, btnH);
            DrawButton(input.DataButton, Localization.Get("BTN_PATIENT_DATA", "[TAB] Patient Data"), input.IsHovering(input.DataButton), Color.FromArgb(255, 0, 180, 255), ba, ta);

            input.ExitButton = new RectangleF(Game.Resolution.Width - 220, btnY, btnW, btnH);
            DrawButton(input.ExitButton, Localization.Get("BTN_EXIT", "[ESC] Exit"), input.IsHovering(input.ExitButton), Color.FromArgb(255, 255, 60, 60), ba, ta);
        }

        private void DrawButton(RectangleF rect, string text, bool hover, Color color, int ba, int ta)
        {
            int alpha = hover ? ba : MathHelper.Clamp(ba - 60, 0, 255);
            NativeUITools.DrawNativeRect(rect.X, rect.Y, rect.Width, rect.Height, Color.FromArgb(alpha, color.R, color.G, color.B));

            if (hover)
                NativeUITools.DrawNativeRect(rect.X, rect.Y + rect.Height - 2, rect.Width, 2, Color.FromArgb(ta, 255, 255, 255));

            float centerX = rect.X + (rect.Width / 2f);
            float centerY = rect.Y + 5f;
            NativeUITools.DrawNativeText(text, centerX, centerY, 0.35f, Color.FromArgb(ta, 255, 255, 255), true);
        }

        private void DrawBodyParts(InspectionState state, BodyPartManager bodyParts)
        {
            var p = GameState.CurrentPatient;
            var activeTool = InventoryManager.ActiveTool;

            foreach (var part in bodyParts.Parts)
            {
                if (part.ScreenPosition == Rage.Vector2.Zero) continue;

                if (activeTool != EmsTreatment.None && part.LinkedEntity == null)
                {
                    bool isAnatomicallyValid = AnatomicalRegistry.IsToolValidForBone(activeTool, part.BoneId);
                    bool isLocalized = AnatomicalRegistry.IsLocalizedTreatment(activeTool);

                    bool needsThisTool = false;

                    if (isLocalized)
                    {
                        needsThisTool = p.Conditions.OfType<PhysicalInjury>()
                            .Any(i => i.Bone == part.BoneId && !i.IsTreated && i.RequiredTreatments.Contains(activeTool));
                    }
                    else
                    {
                        needsThisTool = p.Conditions.Any(c => !c.IsTreated && c.RequiredTreatments.Contains(activeTool));
                    }

                    if (!isAnatomicallyValid || !needsThisTool) continue;
                }

                float scale = bodyParts.GetScale(part);
                float pulse = (float)System.Math.Sin(state.Pulse) * 0.08f + 0.92f;
                bool selected = bodyParts.SelectedPart == part;
                var pos = part.ScreenPosition;

                if (selected || part.IsHovered)
                {
                    float glowSize = 40f * scale * pulse;
                    Color glowColor = selected ? Color.FromArgb(255, 0, 255, 150) : Color.FromArgb(255, 0, 150, 255);
                    NativeUITools.DrawNativeRect(pos.X - glowSize / 2, pos.Y - glowSize / 2, glowSize, glowSize, Color.FromArgb(MathHelper.Clamp(state.BaseAlpha / 4, 0, 255), glowColor));
                }

                float size = 20f * scale;

                Color markerColor;
                if (part.LinkedEntity != null)
                {
                    markerColor = part.IsHovered ? Color.FromArgb(255, 0, 180, 255) : (selected ? Color.FromArgb(255, 0, 255, 150) : Color.FromArgb(255, 200, 200, 200));
                }
                else
                {
                    if (p != null && p.IsBoneInspected(part.BoneId))
                    {
                        bool isInjured = p.Conditions.OfType<PhysicalInjury>().Any(i => i.Bone == part.BoneId && !i.IsTreated);
                        markerColor = isInjured ? Color.FromArgb(255, 220, 60, 60) : Color.FromArgb(255, 0, 200, 100);
                    }
                    else
                    {
                        markerColor = Color.FromArgb(255, 80, 90, 100);
                    }

                    if (selected) markerColor = Color.FromArgb(255, 0, 255, 150);
                    else if (part.IsHovered) markerColor = Color.FromArgb(255, 0, 180, 255);
                }

                NativeUITools.DrawNativeRect(pos.X - size / 2, pos.Y - size / 2, size, size, Color.FromArgb(state.BaseAlpha, markerColor));

                float innerSize = 12f * scale;
                NativeUITools.DrawNativeRect(pos.X - innerSize / 2, pos.Y - innerSize / 2, innerSize, innerSize, Color.FromArgb(state.BaseAlpha, 25, 30, 40));

                if (part.IsHovered)
                {
                    string txt = part.Name.ToUpper();
                    float labelScale = 0.32f;
                    float txtWidth = NativeUITools.MeasureNativeTextWidth(txt, labelScale);
                    float labelY = pos.Y - 40f * scale;

                    NativeUITools.DrawNativeRect(pos.X - txtWidth / 2 - 8, labelY, txtWidth + 16, 25, Color.FromArgb(state.BaseAlpha, 15, 20, 30));

                    NativeUITools.DrawNativeText(txt, pos.X, labelY, labelScale, Color.FromArgb(state.TextAlpha, 255, 255, 255), true);
                }
            }
        }

        private const int MAX_VISIBLE_ACTIONS = 6;
        private void DrawDetailPanel(InspectionState state, BodyPartManager bodyParts, InspectionInput input, List<InspectionAction> actions)
        {
            var part = bodyParts.SelectedPart;
            float offset = (1f - MathHelper.EaseOutCubic(state.PanelSlide)) * 300f;
            float x = Game.Resolution.Width - 360f + offset, y = 180f, w = 340f;

            float actionHeight = 55f;
            float listHeight = MAX_VISIBLE_ACTIONS * actionHeight;
            float headerHeight = 130f;
            float totalH = headerHeight + listHeight + 10f;

            input.PanelBounds = new RectangleF(x, y, w, totalH);

            int pa = MathHelper.Clamp((int)(220 * state.Alpha * state.PanelSlide), 0, 255);
            int pta = MathHelper.Clamp((int)(255 * state.Alpha * state.PanelSlide), 0, 255);

            NativeUITools.DrawNativeRect(x, y, w, totalH, Color.FromArgb(pa, 12, 18, 28));
            NativeUITools.DrawNativeRect(x, y, 3, totalH, Color.FromArgb(255, 0, 255, 150));
            NativeUITools.DrawNativeRect(x, y, w, 50, Color.FromArgb(MathHelper.Clamp(pa / 2, 0, 255), 0, 120, 200));

            // ==========================================
            // CUSTOM PANEL TITLE
            // ==========================================
            string panelTitle = part.Name.ToUpper();
            string cat = BodyInspectionManager.CurrentMenuCategory;

            if (cat != null && cat.StartsWith("QUESTIONS"))
            {
                panelTitle = Localization.Get("MENU_QUESTIONS_TITLE", "QUESTIONS");
            }

            NativeUITools.DrawNativeText(panelTitle, x + 15, y + 10, 0.5f, Color.FromArgb(pta, 255, 255, 255));

            // ==========================================
            // CUSTOM PANEL STATUS
            // ==========================================
            string statusText;
            Color statusColor;
            var p = GameState.CurrentPatient;

            if (cat != null && cat.StartsWith("QUESTIONS"))
            {
                statusText = Localization.Get("DIAG_STATUS_INTERVIEW", "PATIENT INTERVIEW");
                statusColor = Color.FromArgb(255, 0, 180, 255);
            }
            else if (part.LinkedEntity != null)
            {
                statusText = Localization.Get("DIAG_STATUS_EQUIPMENT", "MEDICAL EQUIPMENT");
                statusColor = Color.FromArgb(255, 0, 180, 255);
            }
            else if (part.LinkedEntity == null && p != null && !p.IsBoneInspected(part.BoneId))
            {
                statusText = Localization.Get("DIAG_STATUS_UNKNOWN", "UNKNOWN STATUS");
                statusColor = Color.FromArgb(255, 150, 150, 150);
            }
            else
            {
                var injuries = p?.Conditions.OfType<PhysicalInjury>().Where(i => i.Bone == part.BoneId).ToList();
                var primaryInjury = injuries?.FirstOrDefault(i => !i.IsTreated);
                float totalBleed = injuries?.Where(i => !i.IsTreated).Sum(i => i.BleedSeverity) ?? 0f;

                if (primaryInjury != null)
                {
                    statusText = primaryInjury.Name.ToUpper();
                    if (totalBleed > 2.0f) statusColor = Color.FromArgb(255, 255, 50, 50);
                    else if (totalBleed > 0.5f) statusColor = Color.FromArgb(255, 255, 100, 50);
                    else statusColor = Color.FromArgb(255, 255, 180, 100);
                }
                else if (injuries != null && injuries.Any() && injuries.All(i => i.IsTreated))
                {
                    statusText = Localization.Get("DIAG_STATUS_TREATED", "TREATED");
                    statusColor = Color.FromArgb(255, 255, 180, 0);
                }
                else
                {
                    statusText = Localization.Get("DIAG_STATUS_CLEAR", "CLEAR");
                    statusColor = Color.FromArgb(255, 0, 255, 150);
                }
            }

            NativeUITools.DrawNativeText("●", x + 15, y + 68, 0.3f, statusColor);
            NativeUITools.DrawNativeText(statusText, x + 30, y + 65, 0.4f, statusColor);
            NativeUITools.DrawNativeText(Localization.Get("AVAILABLE_ACTIONS", "AVAILABLE ACTIONS"), x + 15, y + 100, 0.3f, Color.FromArgb(MathHelper.Clamp(pta - 80, 0, 255), 200, 200, 200));

            // ==========================================
            // ACTION BUTTONS
            // ==========================================
            input.PanelActionButtons.Clear();
            float startY = y + 130f;

            int count = actions.Count;
            int startIndex = state.ActionScrollIndex;
            int endIndex = System.Math.Min(startIndex + MAX_VISIBLE_ACTIONS, count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var action = actions[i];
                int visualIndex = i - startIndex;

                RectangleF btnRect = new RectangleF(x + 15, startY, w - 40, 45);
                input.PanelActionButtons.Add(btnRect);

                bool hover = input.IsHoveringPanelAction(visualIndex) && state.PanelSlide > 0.9f && action.Enabled;
                Color baseCol = action.Enabled ? action.Color : Color.FromArgb(255, 60, 65, 70);
                Color drawCol = hover ? Color.FromArgb(pa, baseCol) : Color.FromArgb(MathHelper.Clamp(pa - 40, 0, 255), baseCol.R, baseCol.G, baseCol.B);

                NativeUITools.DrawNativeRect(btnRect.X, btnRect.Y, btnRect.Width, btnRect.Height, drawCol);

                NativeUITools.DrawNativeText(action.Label.ToUpper(), x + 25, startY + 5, 0.35f, action.Enabled ? Color.FromArgb(pta, 255, 255, 255) : Color.FromArgb(pta, 150, 150, 150));
                NativeUITools.DrawNativeText(action.SubLabel, x + 25, startY + 26, 0.25f, action.Enabled ? Color.FromArgb(pta, 200, 220, 230) : Color.FromArgb(pta, 255, 100, 100));

                startY += actionHeight;
            }

            // Scrollbar
            if (count > MAX_VISIBLE_ACTIONS)
            {
                float scrollBarX = x + w - 15f;
                float scrollTrackY = y + 130f;
                float scrollTrackH = listHeight;

                NativeUITools.DrawNativeRect(scrollBarX, scrollTrackY, 4, scrollTrackH, Color.FromArgb(pa / 3, 255, 255, 255));

                float handleH = (scrollTrackH / count) * MAX_VISIBLE_ACTIONS;
                float scrollProgress = (float)startIndex / (count - MAX_VISIBLE_ACTIONS);
                float handleY = scrollTrackY + (scrollProgress * (scrollTrackH - handleH));

                NativeUITools.DrawNativeRect(scrollBarX, handleY, 4, handleH, Color.FromArgb(pta, 0, 180, 255));
            }
        }

        private void DrawDiagnosticPanel(InspectionState state, DiagnosticManager diagnostics)
        {
            float offset = (1f - MathHelper.EaseOutCubic(state.DiagnosticSlide)) * -340f;
            float x = 20f + offset, y = 180f, w = 320f;

            var currentItems = diagnostics.Items.ToList();

            float totalH = 80f;
            foreach (var item in currentItems)
            {
                float h = 45f;
                if (item.Details != null && item.Details.Count > 0) h += item.Details.Count * 18f;
                if (item.HasAction) h += 30f;

                totalH += h;
            }

            int pa = MathHelper.Clamp((int)(220 * state.Alpha * state.DiagnosticSlide), 0, 255);
            int pta = MathHelper.Clamp((int)(255 * state.Alpha * state.DiagnosticSlide), 0, 255);

            NativeUITools.DrawNativeRect(x, y, w, totalH, Color.FromArgb(pa, 12, 18, 28));
            NativeUITools.DrawNativeRect(x, y, 3, totalH, Color.FromArgb(255, 255, 180, 0));
            NativeUITools.DrawNativeRect(x, y, w, 50, Color.FromArgb(MathHelper.Clamp(pa / 2, 0, 255), 200, 100, 0));
            NativeUITools.DrawNativeText(Localization.Get("TITLE_DIAGNOSTICS", "DIAGNOSTICS"), x + 15, y + 10, 0.5f, Color.FromArgb(pta, 255, 255, 255));

            _input.DiagnosticActionButtons.Clear();

            float itemY = y + 65f;
            for (int i = 0; i < currentItems.Count; i++)
            {
                var diag = currentItems[i];

                float thisItemH = 45f;
                if (diag.Details != null && diag.Details.Count > 0) thisItemH += diag.Details.Count * 18f;
                if (diag.HasAction) thisItemH += 30f;

                Color dot = diag.IsNormal ? Color.FromArgb(255, 0, 255, 150) : Color.FromArgb(255, 255, 60, 60);

                NativeUITools.DrawNativeText("●", x + 15, itemY + 3, 0.3f, dot);
                NativeUITools.DrawNativeText(diag.Label, x + 32, itemY, 0.28f, Color.FromArgb(MathHelper.Clamp(pta - 60, 0, 255), 140, 150, 160));
                NativeUITools.DrawNativeText(diag.Value, x + 32, itemY + 18, 0.35f, Color.FromArgb(pta, 210, 220, 230));

                float currentYOffset = 38f;
                if (diag.Details != null && diag.Details.Count > 0)
                {
                    foreach (var step in diag.Details)
                    {
                        NativeUITools.DrawNativeText(step, x + 36, itemY + currentYOffset, 0.24f, Color.FromArgb(pta, 180, 190, 200));
                        currentYOffset += 18f;
                    }
                }

                if (diag.HasAction)
                {
                    float btnY = itemY + currentYOffset + 4f;
                    float btnW = w - 60f, btnH = 26f;
                    RectangleF btnRect = new RectangleF(x + 30, btnY, btnW, btnH);
                    _input.DiagnosticActionButtons.Add(btnRect);

                    bool isHovering = _input.IsHoveringDiagnosticAction(i);
                    Color btnColor = isHovering ? Color.FromArgb(pa, 0, 180, 255) : Color.FromArgb(MathHelper.Clamp(pa - 40, 0, 255), 50, 100, 150);

                    NativeUITools.DrawNativeRect(btnRect.X, btnRect.Y, btnRect.Width, btnRect.Height, btnColor);
                    if (isHovering) NativeUITools.DrawNativeRect(btnRect.X, btnRect.Y + btnRect.Height - 1, btnRect.Width, 1, Color.FromArgb(pta, 100, 200, 255));

                    float centerX = btnRect.X + (btnRect.Width / 2f);
                    NativeUITools.DrawNativeText(diag.ActionButtonText, centerX, btnRect.Y + 3, 0.28f, Color.FromArgb(pta, 255, 255, 255), true);
                }
                else
                {
                    _input.DiagnosticActionButtons.Add(new RectangleF(0, 0, 0, 0));
                }

                itemY += thisItemH;
            }
        }

        private void DrawDataPanel(InspectionState state, PatientDataManager data)
        {
            float offset = (1f - MathHelper.EaseOutCubic(state.DataSlide)) * -340f;
            float x = 20f + offset, y = 180f, w = 320f;

            var currentItems = data.Items;
            var p = GameState.CurrentPatient;

            bool canQuestion = p != null && p.Consciousness != ConsciousnessLevel.Unresponsive;

            float totalH = 80f + (currentItems.Count * 45f);
            if (canQuestion) totalH += 60f;

            int pa = MathHelper.Clamp((int)(220 * state.Alpha * state.DataSlide), 0, 255);
            int pta = MathHelper.Clamp((int)(255 * state.Alpha * state.DataSlide), 0, 255);

            NativeUITools.DrawNativeRect(x, y, w, totalH, Color.FromArgb(pa, 12, 18, 28));
            NativeUITools.DrawNativeRect(x, y, 3, totalH, Color.FromArgb(255, 0, 180, 255));
            NativeUITools.DrawNativeRect(x, y, w, 50, Color.FromArgb(MathHelper.Clamp(pa / 2, 0, 255), 0, 100, 180));
            NativeUITools.DrawNativeText(Localization.Get("TITLE_PATIENT_DATA", "PATIENT DATA"), x + 15, y + 10, 0.5f, Color.FromArgb(pta, 255, 255, 255));

            float itemY = y + 65f;
            foreach (var diag in currentItems)
            {
                NativeUITools.DrawNativeText("●", x + 15, itemY + 3, 0.3f, Color.FromArgb(255, 0, 255, 150));
                NativeUITools.DrawNativeText(diag.Label, x + 32, itemY, 0.28f, Color.FromArgb(MathHelper.Clamp(pta - 60, 0, 255), 140, 150, 160));
                NativeUITools.DrawNativeText(diag.Value, x + 32, itemY + 18, 0.35f, Color.FromArgb(pta, 210, 220, 230));
                itemY += 45f;
            }

            // ==========================================
            // PROMINENT QUESTION BUTTON
            // ==========================================
            if (canQuestion)
            {
                float btnY = itemY + 5f;
                float btnW = w - 40f;
                float btnH = 40f;
                _input.QuestionButton = new RectangleF(x + 20, btnY, btnW, btnH);

                bool isHovering = _input.IsHovering(_input.QuestionButton);
                Color btnColor = isHovering ? Color.FromArgb(pa, 0, 180, 255) : Color.FromArgb(MathHelper.Clamp(pa - 40, 0, 255), 0, 100, 180);

                NativeUITools.DrawNativeRect(_input.QuestionButton.X, _input.QuestionButton.Y, _input.QuestionButton.Width, _input.QuestionButton.Height, btnColor);

                if (isHovering)
                    NativeUITools.DrawNativeRect(_input.QuestionButton.X, _input.QuestionButton.Y + _input.QuestionButton.Height - 2, _input.QuestionButton.Width, 2, Color.FromArgb(pta, 255, 255, 255));

                string btnText = Localization.Get("ACT_QUESTION_PATIENT", "QUESTION PATIENT");
                NativeUITools.DrawNativeText(btnText.ToUpper(), _input.QuestionButton.X + (_input.QuestionButton.Width / 2f), _input.QuestionButton.Y + 8, 0.4f, Color.FromArgb(pta, 255, 255, 255), true);
            }
            else
            {
                _input.QuestionButton = RectangleF.Empty;
            }
        }

        private void DrawCursor(InspectionInput input, InspectionState state, bool hovering)
        {
            float sz = hovering ? 7f : 5f;
            var p = input.MousePosition;
            int ba = state.BaseAlpha;
            NativeUITools.DrawNativeRect(p.X - sz, p.Y - sz, sz * 2, sz * 2, Color.FromArgb(MathHelper.Clamp(ba / 3, 0, 255), 0, 200, 255));
            NativeUITools.DrawNativeRect(p.X - sz / 2, p.Y - sz / 2, sz, sz, Color.FromArgb(MathHelper.Clamp(ba + 55, 0, 255), 255, 255, 255));
        }
    }
}