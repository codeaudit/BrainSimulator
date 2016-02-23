﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using GoodAI.BrainSimulator.Forms;
using GoodAI.Modules.School.Common;
using GoodAI.Core.Utils;
using GoodAI.Modules.School.Worlds;

namespace GoodAI.School.GUI
{
    public partial class LearningTaskSelectionForm : DockContent
    {
        /// <summary>
        /// In order to control itemcheck changes (blinds double clicking, among other things)
        /// </summary>
        bool AuthorizeCheck { get; set; }

        public LearningTaskSelectionForm()
        {
            InitializeComponent();
            learningTaskList.DisplayMember = "DisplayName";
            worldList.DisplayMember = "DisplayName";
        }

        public List<Type> ResultLearningTaskTypes { get; set; }
        public Type ResultWorldType { get; set; }

        private void LearningTaskSelectionForm_Load(object sender, EventArgs e)
        {
            PopulateWorldList();
            PopulateLearningTaskList();
        }

        private void PopulateWorldList()
        {
            var interfaceType = typeof(IWorldAdapter);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) 
                    && !p.IsInterface 
                    && !p.IsAbstract
                    && IsAdmitted(p));
            foreach (Type type in types)
            {
                worldList.Items.Add(new TypeListItem(type));
            }
            if (worldList.Items.Count > 0)
                worldList.SelectedIndex = 0;
        }

        // True if the world should appear in the GUI
        // Used to suppress worlds that should not be used
        private bool IsAdmitted(Type p)
        {
            return p != typeof(PlumberWorld);
        }

        private void PopulateLearningTaskList()
        {
            learningTaskList.Items.Clear();
            Type selectedWorldType = (worldList.SelectedItem as TypeListItem).Type;
            foreach (var entry in LearningTaskFactory.KnownLearningTasks)
            {
                Type learningTaskType = entry.Key;
                List<Type> worldTypes = entry.Value;
                if (ContainsType(worldTypes, selectedWorldType))
                if (worldTypes.Contains(selectedWorldType))
                { 
                    learningTaskList.Items.Add(new LearningTaskListItem(learningTaskType));
                }
            }

            if (learningTaskList.Items.Count > 0)
                learningTaskList.SelectedIndex = 0;
        }

        private bool ContainsType(List<Type> worldTypes, Type selectedWorldType)
        {
            foreach (Type type in worldTypes)
            {
                if (selectedWorldType == type || selectedWorldType.IsSubclassOf(type))
                {
                    return true;
                }
            }
            return false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ResultLearningTaskTypes = new List<Type>();
            foreach (var item in learningTaskList.CheckedItems)
            {
                ResultLearningTaskTypes.Add((item as TypeListItem).Type);
            }
            ResultWorldType = (worldList.SelectedItem as TypeListItem).Type;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            ResultLearningTaskTypes = null;
            ResultWorldType = null;
            Close();
        }

        private void learningTaskList_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateLearningTaskDescription();
        }

        private void UpdateLearningTaskDescription()
        {
            if (learningTaskList.SelectedItem == null)
            {
                learningTaskDescription.Url = null;
            }
            else
            { 
                const string HTML_DIRECTORY = @"Resources\html";
                string htmlFileName = (learningTaskList.SelectedItem as LearningTaskListItem).HTMLFileName;
                string fullPath = MyResources.GetMyAssemblyPath() + "\\" + HTML_DIRECTORY + "\\" + htmlFileName;
                learningTaskDescription.Navigate(fullPath);
            }
        }

        private void worldList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateLearningTaskList();
        }

        // Implements "check only when box clicked" behavior in the checkedlistbox
        // See http://stackoverflow.com/questions/2093961/checkedlistbox-control-only-checking-the-checkbox-when-the-actual-checkbox-is
        private void learningTaskList_MouseClick(object sender, MouseEventArgs e)
        {
            const int CHECK_BOX_WIDTH = 16;
            for (int i = 0; i < this.learningTaskList.Items.Count; i++)
            {
                Rectangle rec = this.learningTaskList.GetItemRectangle(i);
                rec.Width = CHECK_BOX_WIDTH;

                if (rec.Contains(e.Location))
                {
                    AuthorizeCheck = true;
                    bool newValue = !this.learningTaskList.GetItemChecked(i);
                    this.learningTaskList.SetItemChecked(i, newValue);
                    AuthorizeCheck = false;

                    return;
                }
            }
        }

        // Implements "check only when box clicked" behavior in the checkedlistbox
        private void learningTaskList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!AuthorizeCheck)
                e.NewValue = e.CurrentValue; //check state change was not through authorized actions
        }
    }

    class TypeListItem
    {
        public Type Type { get; set; }

        public TypeListItem(Type type)
        {
            Type = type;
        }

        public String DisplayName
        {
            get
            {
                DisplayNameAttribute attribute = Type.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
                return attribute != null ? attribute.DisplayName : "Display name missing!";
            }
        }
    }

    class LearningTaskListItem : TypeListItem
    {
        public LearningTaskListItem(Type type) : base(type)
        {
        }

        public string HTMLFileName 
        { 
            get
            {
                return Type.Name + ".html";
            }
        }
    }
}
