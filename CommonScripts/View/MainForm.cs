﻿using CommonScripts.CustomComponent.ScriptListBox;
using CommonScripts.Model.Pojo;
using CommonScripts.Model.Pojo.Base;
using CommonScripts.Presenter;
using CommonScripts.Model.Service;
using CommonScripts.View.Interfaces;
using MetroSet_UI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CommonScripts.View
{
    public partial class MainForm : MetroSetForm, IMainView
    {
        public MainPresenter Presenter { get; set; }
        private ScriptListAdapter scriptListAdapter;

        public MainForm()
        {
            InitializeComponent();
            scriptListAdapter = new ScriptListAdapter(StyleManager, pnlScripts);
            scriptListAdapter.EditClicked += EditScript;
            scriptListAdapter.RemoveClicked += RemoveScript;
            scriptListAdapter.StatusClicked += ChangeScriptStatus;

            ListenKeysService.GetInstance().Run();
        }
        //Helps with the refresh of the UI when resizing the window
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        #region Events
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Presenter.LoadSettings();
        }

        private void AddScript(object sender, EventArgs e)
        {
            ShowScriptForm(null, (ScriptAbs addedScript, bool hasScriptTypeChanged /*Unused, just used when editting*/) => {
                if (Presenter.AddScript(addedScript))
                {
                    scriptListAdapter.AddItem(addedScript);
                }
            });
        }

        private void ChangeScriptStatus(ScriptAbs script)
        {
            ScriptStatus newStatus = Presenter.ChangeScriptStatus(script);
            scriptListAdapter.ChangeScriptStatus(script.Id, newStatus);
        }

        private void RemoveScript(ScriptAbs script)
        {
            DialogResult r = MetroSetMessageBox.Show(this, "Do you want to remove the script?", "Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (r == DialogResult.Yes)
            {
                string scriptId = script.Id;
                if (Presenter.RemoveScript(scriptId))
                {
                    scriptListAdapter.RemoveItem(scriptId);
                }
            }
        }

        private void EditScript(ScriptAbs script)
        {
            ShowScriptForm(script, (ScriptAbs editedScript, bool hasScriptTypeChanged) => {
                if (Presenter.EditScript(editedScript))
                {
                    scriptListAdapter.EditItem(editedScript, hasScriptTypeChanged);
                }
            });
        }
        #endregion

        private void ShowScriptForm(ScriptAbs script, Action<ScriptAbs, bool> postAction)
        {
            ScriptForm scriptForm = new ScriptForm(styleManager, script);
            if (scriptForm.ShowDialog() == DialogResult.OK)
            {
                bool hasScriptTypeChanged = script != null && scriptForm.HasScriptTypeChanged;
                postAction(scriptForm.GetScript(), hasScriptTypeChanged);
            }
        }

        public void ShowScripts(IList<ScriptAbs> scripts)
        {
            scriptListAdapter.CreateWithList(scripts);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void appNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            
        }
    }
}
