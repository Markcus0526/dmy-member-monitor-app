using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.INMMessage;
using System.DirectoryServices.AccountManagement;
using System.Collections;
using System.Threading;

namespace AgentEngine
{
    public class UserManage
    {
        object userobj = new object();

        ArrayList userList = new ArrayList();

        PrincipalContext insPrincipalContext = new PrincipalContext(ContextType.Machine);

        public UserManage()
        {
        }

        private void SearchUsers(UserPrincipal parUserPrincipal)
        {
            PrincipalSearcher insPrincipalSearcher = new PrincipalSearcher();
            insPrincipalSearcher.QueryFilter = parUserPrincipal;
            PrincipalSearchResult<Principal> results = insPrincipalSearcher.FindAll();
            foreach (Principal p in results)
            {
                M6UserItem userItem = new M6UserItem();
                userItem.strUserName = p.Name;

                UserPrincipal insUserPrincipal = (UserPrincipal)p;
                List<Principal> insListPrincipal = new List<Principal>();

                foreach (Principal pr in insUserPrincipal.GetGroups())
                {
                    userItem.strPrivilege += pr.Name + ",";
                }

                if (userItem.strPrivilege.Length > 0)
                {
                    userItem.strPrivilege = userItem.strPrivilege.Substring(0, userItem.strPrivilege.Length - 1);
                    userItem.strPassword = "";
                    userItem.strLogonState = "";
                    userItem.strLogonTime = DateTime.MinValue;
                    userItem.strLogoffTime = DateTime.MinValue;

                    userList.Add(userItem);
                }
            }
        }

        public void GetUserList(M6UserSet userSet)
        {
            Monitor.Enter(userobj);

            userSet.userList.Clear();
            userList.Clear();

            UserPrincipal insUserPrincipal = new UserPrincipal(insPrincipalContext);
            insUserPrincipal.Name = "*";
            SearchUsers(insUserPrincipal);

            foreach (M6UserItem item in userList)
            {
                M6UserItem newitem = new M6UserItem(item);
                userSet.userList.Add(newitem);
            }

            Monitor.Exit(userobj);
        }

        public UserPrincipal GetUserPrincipal(M6UserItem item)
        {
            UserPrincipal insUserPrincipal = new UserPrincipal(insPrincipalContext);
            insUserPrincipal.Name = "*";

            PrincipalSearcher insPrincipalSearcher = new PrincipalSearcher();
            insPrincipalSearcher.QueryFilter = insUserPrincipal;
            PrincipalSearchResult<Principal> results = insPrincipalSearcher.FindAll();
            foreach (Principal p in results)
            {
                if (p.Name == item.strUserName)
                    return (UserPrincipal)p;
            }

            return null;
        }

        public void SetUserPass(M6UserItem newUserItem)
        {
            UserPrincipal insUserPrincipal = GetUserPrincipal(newUserItem);

            if (insUserPrincipal == null)
                return;

            try
            {
                insUserPrincipal.SetPassword(newUserItem.strPassword);
            }
            catch (Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }

        public void AddUser(M6UserItem addItem)
        {
            UserPrincipal insUserPrincipal = new UserPrincipal(insPrincipalContext);
            if (addItem.strUserName != "")
            {
                insUserPrincipal.DelegationPermitted = true;
                insUserPrincipal.Enabled = true;
                insUserPrincipal.Name = addItem.strUserName;
                insUserPrincipal.PasswordNeverExpires = true;
                insUserPrincipal.PasswordNotRequired = true;
                insUserPrincipal.SamAccountName = addItem.strUserName;
                insUserPrincipal.SetPassword(addItem.strPassword);

                try
                {
                    insUserPrincipal.Save();

                    // **************** Permite ******************** //
                    String strPermit = addItem.strPrivilege;
                    GroupPrincipal groupPermit;
                    while (true)
                    {
                        int idx = strPermit.IndexOf(',');
                        if (idx == -1)
                            break;

                        String strOnePermit = strPermit.Substring(0, idx);

                        groupPermit = GroupPrincipal.FindByIdentity(insPrincipalContext, strOnePermit);
                        groupPermit.Members.Add(insUserPrincipal);
                        groupPermit.Save();

                        strPermit = strPermit.Substring(idx + 1, strPermit.Length - idx - 1);
                    }

                    groupPermit = GroupPrincipal.FindByIdentity(insPrincipalContext, strPermit);
                    groupPermit.Members.Add(insUserPrincipal);
                    groupPermit.Save();

                    // ***************************************************************** //

                    insUserPrincipal.Dispose();
                }
                catch (Exception ex)
                {
                    ComMisc.LogErrors(ex.ToString());
                }
            }
        }

        public void DeleteUser(M6UserItem delItem)
        {
            UserPrincipal insUserPrincipal = GetUserPrincipal(delItem);
            try
            {
                insUserPrincipal.Delete();
                insUserPrincipal.Dispose();
            }
            catch (Exception ex)
            {
                ComMisc.LogErrors(ex.ToString());
            }
        }
    }
}
