'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports Microsoft.Win32

Module RegistryHelper

#If TESTING = 1 Then
    Private Const BASE_REG_PATH As String = "Software\Sbs\Routeur\#Testing"
#Else
    Private Const BASE_REG_PATH As String = "Software\Sbs\Routeur"
#End If

    Private Const USER_SUB_KEY As String = "Users"
    Private Const USER_NICK As String = "Nick"
    Private Const USER_NUMBOAT As String = "NumBoat"
    Private Const USER_PASSWORD As String = "Password"
    Private Const USER_MINE As String = "ISMINE"
    Private Const KEY_LAST_BOAT As String = "LastBoat"
    Private Const USER_IDP As String = "IDPlayer"
    Private Const USER_NEWSTYLE As String = "NewStyle"
    Private Const USER_EMAIL As String = "Email"

    Public Sub DeleteUser(ByVal P As RegistryPlayerInfo)

        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(BASE_REG_PATH & "\" & USER_SUB_KEY, True)

        If Not Reg Is Nothing Then
            Reg.DeleteSubKey(P.Nick)
        End If

        Return

    End Sub

    Public Function GetLastPlayer() As String
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(BASE_REG_PATH, False)

        If Not Reg Is Nothing Then
            Return CStr(Reg.GetValue(KEY_LAST_BOAT))
        End If

        Return ""
    End Function

    Public Sub SetLastPlayer(ByVal BoatName As String)
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(BASE_REG_PATH, True)

        If Not Reg Is Nothing Then
            Reg.SetValue(KEY_LAST_BOAT, BoatName)
        End If

        Return
    End Sub

    Public Function GetUserPassword(ByVal UserName As String) As String

        Dim PasswordKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(PasswordKey, False)
        Dim RetString As String = ""

        If Reg Is Nothing Then
            Return ""
        End If

        Dim Value As Object = Reg.GetValue(UserName)

        If Value Is Nothing Then
            Return ""
        Else
            Return CStr(Value)
        End If

        Return RetString

    End Function

    Public Sub LoadPlayers(ByVal List As IList(Of RegistryPlayerInfo))

        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(BASE_REG_PATH & "\" & USER_SUB_KEY, False)

        If Reg Is Nothing Then
            Return
        End If
        Dim names() As String = Reg.GetSubKeyNames()

        For Each Name In names
            Dim P As RegistryPlayerInfo = New RegistryPlayerInfo(Name)
            List.Add(P)
        Next

        Return


    End Sub

    Public Sub LoadUserInfo(ByVal User As RegistryPlayerInfo, ByVal BoatNick As String)
        If BoatNick Is Nothing Then
            Return
        End If
        Dim UserKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY & "\" & BoatNick
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(UserKey, False)

        If Reg Is Nothing Then
            Return
        End If

        With User
            .IsLoading = True
            .IDP = CStr(Reg.GetValue(USER_IDP, ""))
            .Nick = BoatNick
            .IDU = CInt(Reg.GetValue(USER_NUMBOAT, 0))
            .IsMine = CInt(Reg.GetValue(USER_MINE, 0)) = 1
            .PasswordDecrypt = CType(Reg.GetValue(USER_PASSWORD, Nothing), Byte())
            .Email = CStr(Reg.GetValue(USER_EMAIL, ""))
            .NewStyle = CInt(Reg.GetValue(USER_NEWSTYLE, 0)) = 1
            .IsLoading = False
        End With

        Return
    End Sub

    Public Sub SaveUserInfo(ByVal User As RegistryPlayerInfo)

        If User.Nick Is Nothing Then
            Return
        End If
        Dim UserKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY & "\" & User.Nick
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(UserKey, True)

        If Reg Is Nothing Then
            Reg = Registry.CurrentUser.CreateSubKey(UserKey)
        End If

        With User

            Reg.SetValue(USER_NUMBOAT, .IDU, RegistryValueKind.DWord)
            Reg.SetValue(USER_MINE, If(.IsMine, 1, 0), RegistryValueKind.DWord)
            Reg.SetValue(USER_PASSWORD, .PasswordEncrypt, RegistryValueKind.Binary)
            If Not .IDP Is Nothing Then
                Reg.SetValue(USER_IDP, .IDP, RegistryValueKind.String)
            End If
            If Not .Email Is Nothing Then
                Reg.SetValue(USER_EMAIL, .Email, RegistryValueKind.String)
            End If
            Reg.SetValue(USER_NEWSTYLE, If(.NewStyle, 1, 0), RegistryValueKind.DWord)
        End With

        Return
    End Sub


    Public Function SetUserPassword(ByVal UserName As String, ByVal Password As String) As Boolean

        Dim PasswordKey As String = BASE_REG_PATH & "\" & USER_SUB_KEY
        Dim Reg As RegistryKey = Registry.CurrentUser.OpenSubKey(PasswordKey, True)
        Dim RetString As String = ""

        If Reg Is Nothing Then
            Reg = Registry.CurrentUser.CreateSubKey(PasswordKey, RegistryKeyPermissionCheck.ReadWriteSubTree)
            If Reg Is Nothing Then
                Return False
            End If
        End If

        Try
            Reg.SetValue(UserName, Password, RegistryValueKind.String)
            Return True
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbCrLf & ex.StackTrace, "Error setting password")
        End Try
        Return False


    End Function


End Module
