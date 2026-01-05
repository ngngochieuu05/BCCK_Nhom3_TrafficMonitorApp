using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TrafficMonitorApp.Utils;

namespace TrafficMonitorApp.GUI
{
    /// <summary>
    /// Ví d? c? th? cách áp d?ng ProfessionalUIStyler vào MainForm
    /// Specific example of how to apply ProfessionalUIStyler to MainForm
    /// </summary>
    public static class MainFormProfessionalStyling
    {
        /// <summary>
        /// Áp d?ng Professional UI Style cho MainForm
        /// G?i method này trong Form_Load ho?c OnLoad c?a MainForm
        /// </summary>
        public static void ApplyProfessionalStyle(Form mainForm)
        {
            try
            {
                // ===== B??C 1: APPLY CHUNG CHO TOÀN B? FORM =====
                ProfessionalUIStyler.ApplyFormStyle(mainForm);
                
                // ===== B??C 2: STYLE BUTTONS - ?I?U KHI?N =====
                StyleControlButtons(mainForm);
                
                // ===== B??C 3: STYLE BUTTONS - KHÁC =====
                StyleOtherButtons(mainForm);
                
                // ===== B??C 4: STYLE PANELS VÀ GROUPBOXES =====
                StylePanelsAndGroupBoxes(mainForm);
                
                // ===== B??C 5: STYLE INPUT CONTROLS =====
                StyleInputControls(mainForm);
                
                // ===== B??C 6: STYLE LABELS =====
                StyleLabels(mainForm);
                
                Console.WriteLine("[MainForm] Professional UI Style applied successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainForm] Error applying professional style: {ex.Message}");
            }
        }
        
        /// <summary>
        /// B??C 1: Style các Button ?i?u khi?n (Start, Pause, Stop)
        /// </summary>
        private static void StyleControlButtons(Form form)
        {
            // Tìm các button theo Name
            var btnStart = form.Controls.Find("btnStart", true).FirstOrDefault() as Button;
            var btnPause = form.Controls.Find("btnPause", true).FirstOrDefault() as Button;
            var btnStop = form.Controls.Find("btnStop", true).FirstOrDefault() as Button;
            
            if (btnStart != null)
            {
                ProfessionalUIStyler.StyleButton(btnStart, ButtonStyle.Success, "?");
                btnStart.Size = new Size(60, 40);
                btnStart.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }
            
            if (btnPause != null)
            {
                ProfessionalUIStyler.StyleButton(btnPause, ButtonStyle.Warning, "?");
                btnPause.Size = new Size(60, 40);
                btnPause.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }
            
            if (btnStop != null)
            {
                ProfessionalUIStyler.StyleButton(btnStop, ButtonStyle.Danger, "?");
                btnStop.Size = new Size(60, 40);
                btnStop.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            }
        }
        
        /// <summary>
        /// B??C 2: Style các Button khác (Browse, Select, Reset, etc)
        /// </summary>
        private static void StyleOtherButtons(Form form)
        {
            var buttonNames = new[]
            {
                "btnLoadModel", "btnLoadData", "btnBrowseData", "btnBrowseModel", "btnBrowseExport",
                "btnSelectZone", "btnSelectLine", "btnShowZone", "btnResetZone",
                "btnGenerateCharts", "btnHelp", "btnSettings", "btnAbout",
                "btnStartParking", "btnStopParking", "btnViewParkingReport",
                "btnAdminDashboard"
            };
            
            var icons = new Dictionary<string, string>
            {
                { "btnLoadModel", "??" },
                { "btnLoadData", "??" },
                { "btnBrowseData", "??" },
                { "btnBrowseModel", "??" },
                { "btnBrowseExport", "??" },
                { "btnSelectZone", "??" },
                { "btnSelectLine", "??" },
                { "btnShowZone", "??" },
                { "btnResetZone", "??" },
                { "btnGenerateCharts", "??" },
                { "btnHelp", "?" },
                { "btnSettings", "??" },
                { "btnAbout", "??" },
                { "btnStartParking", "??" },
                { "btnStopParking", "?" },
                { "btnViewParkingReport", "??" },
                { "btnAdminDashboard", "??" }
            };
            
            foreach (var btnName in buttonNames)
            {
                var btn = form.Controls.Find(btnName, true).FirstOrDefault() as Button;
                if (btn != null)
                {
                    string icon = icons.ContainsKey(btnName) ? icons[btnName] : "";
                    ProfessionalUIStyler.StyleButton(btn, ButtonStyle.Secondary, icon);
                    btn.Size = new Size(160, 40);
                }
            }
        }
        
        /// <summary>
        /// B??C 3: Style Panels và GroupBoxes
        /// </summary>
        private static void StylePanelsAndGroupBoxes(Form form)
        {
            var splitContainer = form.Controls.Find("splitContainer", true).FirstOrDefault() as SplitContainer;
            
            if (splitContainer != null)
            {
                // Style left panel (Sidebar)
                ProfessionalUIStyler.StylePanel(splitContainer.Panel1, isContainer: false);
                
                // Style right panel (Video display)
                ProfessionalUIStyler.StylePanel(splitContainer.Panel2, isContainer: false);
            }
            
            // Style t?t c? GroupBoxes
            var allGroupBoxes = GetAllControlsOfType<GroupBox>(form);
            foreach (var gb in allGroupBoxes)
            {
                ProfessionalUIStyler.StyleGroupBox(gb);
            }
        }
        
        /// <summary>
        /// B??C 4: Style Input Controls (TextBox, NumericUpDown, CheckBox, etc)
        /// </summary>
        private static void StyleInputControls(Form form)
        {
            var allTextBoxes = GetAllControlsOfType<TextBox>(form);
            foreach (var txt in allTextBoxes)
            {
                ProfessionalUIStyler.StyleTextBox(txt);
            }
            
            var allNumericUpDown = GetAllControlsOfType<NumericUpDown>(form);
            foreach (var num in allNumericUpDown)
            {
                ProfessionalUIStyler.StyleNumericUpDown(num);
            }
            
            var allCheckBoxes = GetAllControlsOfType<CheckBox>(form);
            foreach (var chk in allCheckBoxes)
            {
                ProfessionalUIStyler.StyleCheckBox(chk);
            }
            
            var allComboBoxes = GetAllControlsOfType<ComboBox>(form);
            foreach (var cmb in allComboBoxes)
            {
                ProfessionalUIStyler.StyleComboBox(cmb);
            }
        }
        
        /// <summary>
        /// B??C 5: Style Labels
        /// </summary>
        private static void StyleLabels(Form form)
        {
            var statusLabels = new[] { "lblStatus", "lblScheduleStatus", "lblParkingStatus", "lblCurrentVehicles", "lblVideoTime" };
            
            foreach (var lblName in statusLabels)
            {
                var lbl = form.Controls.Find(lblName, true).FirstOrDefault() as Label;
                if (lbl != null)
                {
                    // Gi? nguyên màu c?a status label n?u ?ã ???c set
                    if (lbl.ForeColor == Color.Black || lbl.ForeColor == Color.White)
                    {
                        lbl.ForeColor = ProfessionalUIStyler.TextWhite;
                    }
                    lbl.Font = new Font("Segoe UI", 10);
                    lbl.BackColor = Color.Transparent;
                }
            }
            
            // FPS label - Large value
            var lblFPS = form.Controls.Find("lblFPS", true).FirstOrDefault() as Label;
            if (lblFPS != null)
            {
                ProfessionalUIStyler.StyleLabel(lblFPS, LabelStyle.Value);
            }
            
            // Count labels - Value style
            var countLabels = new[] { "txtTotalCount", "txtCarCount", "txtMotorCount", "txtBusCount", "txtBicycleCount" };
            foreach (var lblName in countLabels)
            {
                var ctrl = form.Controls.Find(lblName, true).FirstOrDefault();
                if (ctrl is TextBox txt)
                {
                    txt.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                    txt.ForeColor = ProfessionalUIStyler.PrimaryBlue;
                    txt.BackColor = ProfessionalUIStyler.PanelBackground;
                    txt.BorderStyle = BorderStyle.None;
                }
            }
        }
        
        /// <summary>
        /// Helper method: L?y t?t c? control c?a m?t type c? th? (?? quy)
        /// Helper method: Get all controls of a specific type (recursive)
        /// </summary>
        private static List<T> GetAllControlsOfType<T>(Control container) where T : Control
        {
            var result = new List<T>();
            
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is T typedControl)
                {
                    result.Add(typedControl);
                }
                
                if (ctrl.HasChildren)
                {
                    result.AddRange(GetAllControlsOfType<T>(ctrl));
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// T??ng t? cho DashboardForm
    /// Similar for DashboardForm
    /// </summary>
    public static class DashboardFormProfessionalStyling
    {
        public static void ApplyProfessionalStyle(Form dashboardForm)
        {
            try
            {
                // Apply form style
                ProfessionalUIStyler.ApplyFormStyle(dashboardForm);
                
                // Style all buttons
                StyleQuickActionButtons(dashboardForm);
                
                // Style DataGridView
                StyleDataGridViews(dashboardForm);
                
                Console.WriteLine("[DashboardForm] Professional UI Style applied successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardForm] Error applying professional style: {ex.Message}");
            }
        }
        
        private static void StyleQuickActionButtons(Form form)
        {
            var btnNames = new[]
            {
                "btnStartMonitoring", "btnViewReports", "btnSettings", "btnManageData"
            };
            
            var buttonStyles = new Dictionary<string, ButtonStyle>
            {
                { "btnStartMonitoring", ButtonStyle.Success },
                { "btnViewReports", ButtonStyle.Primary },
                { "btnSettings", ButtonStyle.Secondary },
                { "btnManageData", ButtonStyle.Secondary }
            };
            
            foreach (var btnName in btnNames)
            {
                var btn = form.Controls.Find(btnName, true).FirstOrDefault() as Button;
                if (btn != null)
                {
                    var style = buttonStyles.ContainsKey(btnName) ? buttonStyles[btnName] : ButtonStyle.Secondary;
                    ProfessionalUIStyler.StyleButton(btn, style);
                    btn.Size = new Size(220, 55);
                    btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                }
            }
        }
        
        private static void StyleDataGridViews(Form form)
        {
            var dgvs = GetAllControlsOfType<DataGridView>(form);
            foreach (var dgv in dgvs)
            {
                ProfessionalUIStyler.StyleDataGridView(dgv);
            }
        }
        
        private static List<T> GetAllControlsOfType<T>(Control container) where T : Control
        {
            var result = new List<T>();
            
            foreach (Control ctrl in container.Controls)
            {
                if (ctrl is T typedControl)
                {
                    result.Add(typedControl);
                }
                
                if (ctrl.HasChildren)
                {
                    result.AddRange(GetAllControlsOfType<T>(ctrl));
                }
            }
            
            return result;
        }
    }
}
