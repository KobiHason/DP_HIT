﻿using System;
using System.Windows.Forms;
using FacebookWrapper.ObjectModel;
using FacebookWrapper;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace facebookApi
{
    public partial class MainAppForm : Form 
    {
        public enum eReligion
        {
            Christian, Muslim, Jewish
        }

        private readonly ISet<eReligion> r_religionsToPresent = new HashSet<eReligion>();
        private readonly ISet<User.eGender> r_genderToPresent = new HashSet<User.eGender>();
        private readonly ISet<User.eRelationshipStatus> r_relationshipStatusesToPresent = new HashSet<User.eRelationshipStatus>();

        private User m_LoggedInUser;
        private readonly string k_ApplicationID = "1450160541956417";
        private readonly string[] r_FaceboookPermissions = {"public_profile", /*"user_religion",*/ "user_photos", "user_gender", "user_friends", "publish_actions"};

        private void loginAndInit()
        {
            this.m_filtersHeadlineLabel.Visible = true;

            try
            {
                LoginResult result = FacebookService.Login(k_ApplicationID, r_FaceboookPermissions);

                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    this.m_LoggedInUser = result.LoggedInUser;
                    this.fetchUserInfo();
                    this.enableControls();
                    this.m_loginButton.Enabled = false;
                    this.m_logoutButton.Enabled = true;
                }
                else
                {
                    MessageBox.Show(string.Format("Error with login: \n{0}", result.ErrorMessage));
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Error with login: \n{0}", e.Message));
            }

            this.m_filtersHeadlineLabel.Visible = false;
        }

        private void fetchUserInfo()
        {
            this.m_loggedInUserPictureBox.LoadAsync(this.m_LoggedInUser.PictureNormalURL);

            if (this.m_LoggedInUser.Statuses != null && this.m_LoggedInUser.Statuses.Count > 0)
            {
                this.m_loggedInUserPictureBox.Text = this.m_LoggedInUser.Statuses[0].Message;
            }

            this.m_userNameLabel.Text = string.Format("Welcome  {0} {1}", this.m_LoggedInUser.FirstName, this.m_LoggedInUser.LastName);
            this.enableControls();

        }

        private void displayAllFriends()
        {
            foreach (User friend in m_LoggedInUser.Friends)
            {
                friend.ReFetch(DynamicWrapper.eLoadOptions.Full);
            }

            if (m_LoggedInUser.Friends.Count == 0)
            {
                MessageBox.Show("No Friends to retrieve :(");
            }
        }

        private void enableControls()
        {
            this.m_loggedInUserPictureBox.Visible = true;
            this.m_loginButton.Enabled = false;
            this.m_loginButton.Visible = false;
            this.m_logoutButton.Enabled = true;
            this.m_friendPictureBox.Visible = true;
        }

        public MainAppForm()
        {
            InitializeComponent();
            generateCheckBoxesandAddToGroupBox<User.eRelationshipStatus>(Enum.GetValues(typeof(User.eRelationshipStatus)).OfType<User.eRelationshipStatus>().ToList(), m_relationshipStatusFilterGroupBox, new EventHandler(this.relationshipStatus_CheckedChanged));
            generateCheckBoxesandAddToGroupBox<eReligion>(Enum.GetValues(typeof(eReligion)).OfType<eReligion>().ToList(), m_religionFilterGroupBox, new EventHandler(this.religion_CheckedChanged));
            generateCheckBoxesandAddToGroupBox<User.eGender>(Enum.GetValues(typeof(User.eGender)).OfType<User.eGender>().ToList(), m_genderFilterGroupBox, new EventHandler(this.gender_CheckedChanged));
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            this.loginAndInit();
        }

        private void logoutButton_Click(object sender, EventArgs e)
        {
            FacebookService.Logout(postLogout);
            this.m_loginButton.Enabled = true;
            this.m_loginButton.Visible = true;


        }

        private void postLogout()
        {
            m_loginButton.Enabled = true;
            m_logoutButton.Enabled = false;
            m_LoggedInUser = null;
        }


        private void searchButton_Click(object sender, EventArgs e)
        {
            ISet<User> usersToPresent = new HashSet<User>();

            foreach (eReligion religion in r_religionsToPresent) 
            {
                foreach(User friend in m_LoggedInUser.Friends) 
                {
                    if (friend.Religion != null && friend.Religion.Equals(religion.ToString())) 
                    {
                        usersToPresent.Add(friend);
                    }
                }
            }

            foreach (User.eGender gender in r_genderToPresent)
            {
                foreach (User friend in m_LoggedInUser.Friends)
                {
                    if (friend.Gender.Equals(gender))
                    {
                        usersToPresent.Add(friend);
                    }
                }
            }

            foreach (User.eRelationshipStatus relationshipStatus in r_relationshipStatusesToPresent)
            {
                foreach (User friend in m_LoggedInUser.Friends)
                {
                    if (friend.RelationshipStatus.Equals(relationshipStatus))
                    {
                        usersToPresent.Add(friend);
                    }
                }
            }

            m_filteredFriends.Items.Clear();

            foreach (User userToPresent in usersToPresent) {
                m_filteredFriends.Items.Add(userToPresent.Name);
            }
        }


        private void postButton_Click(object sender, EventArgs e)
        {
            string postText = m_postTextBox.Text;

            if (string.IsNullOrEmpty(postText)) {
                MessageBox.Show("Adding an empty post is not allowed!!!");
            } else {
                Status postedStatus = m_LoggedInUser.PostStatus(postText);
                MessageBox.Show("Status Posted! ID: " + postedStatus.Id);
            }
        }

        private void linkFriends_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           
        }

    
        private void fetchFriends()
        {
            m_friendslistBox.Items.Clear();
            m_friendslistBox.DisplayMember = "Name";
            foreach (User friend in m_LoggedInUser.Friends)
            {
                m_friendslistBox.Items.Add(friend);
                friend.ReFetch(DynamicWrapper.eLoadOptions.Full);
            }

            if (m_LoggedInUser.Friends.Count == 0)
            {
                MessageBox.Show("No Friends to retrieve :(");
            }
        }
    

        private void friendslistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            displaySelectedFriend();
        }

        private void displaySelectedFriend()
        {
            if (m_friendslistBox.SelectedItems.Count == 1)
            {
                User selectedFriend = m_friendslistBox.SelectedItem as User;

                if (selectedFriend.PictureNormalURL != null)
                {
                    m_friendPictureBox.LoadAsync(selectedFriend.PictureNormalURL);
                }
            }
        }

        private void religion_CheckedChanged(object sender, EventArgs e)
        {
            filterCheckBoxChanged<eReligion>(sender, r_religionsToPresent);
        }

        private void gender_CheckedChanged(object sender, EventArgs e)
        {
            filterCheckBoxChanged<User.eGender>(sender, r_genderToPresent);
        }

        private void relationshipStatus_CheckedChanged(object sender, EventArgs e)
        {
            filterCheckBoxChanged<User.eRelationshipStatus>(sender, r_relationshipStatusesToPresent);
        }

        private void filterCheckBoxChanged<T>(object sender , ISet<T> setToEdit) where T : struct, System.IConvertible 
        {
            CheckBox checkbox = (CheckBox)sender;

            T value = (T)Enum.Parse(typeof(T), checkbox.Text);

            if (checkbox.Checked)
            {
                setToEdit.Add(value);
            }
            else
            {
                setToEdit.Remove(value);
            }
        }

        private void generateCheckBoxesandAddToGroupBox<T>(List<T> a, GroupBox groupBox, EventHandler eventHandler) where T : struct, System.IConvertible
        {
            int checkBoxLocationX = 6;
            int checkBoxLocationYStart = 21;

            int i = 0;

            foreach (T value in a)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.AutoSize = true;
                checkBox.Location = new Point(checkBoxLocationX, checkBoxLocationYStart + (i * 23));
                checkBox.Name = value.ToString();
                checkBox.TabIndex = i + 1;
                checkBox.Text = value.ToString();
                checkBox.UseVisualStyleBackColor = true;
                checkBox.CheckedChanged += eventHandler;
                groupBox.Controls.Add(checkBox);
                i++;
            }
        }

        private void m_postTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void m_loggedInUserPictureBox_Click(object sender, EventArgs e)
        {

        }

        private void fetchFriendsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            fetchFriends();
        }

        private void m_genderFilterGroupBox_Enter(object sender, EventArgs e)
        {

        }

        private void m_relationshipStatusFilterGroupBox_Enter(object sender, EventArgs e)
        {

        }

        private void m_religionFilterGroupBox_Enter(object sender, EventArgs e)
        {

        }

   
}

}