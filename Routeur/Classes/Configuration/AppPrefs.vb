Imports System.ComponentModel
Imports System.IO

Public Class AppPrefs

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged


    Public Property AutoStartRouterAtMeteoUpdate As Boolean = False

    Public Shared Sub Load(ByVal Prefs As AppPrefs)

        Dim BaseFileName As String = IO.Path.Combine(RouteurModel.BaseFileDir, "AppPrefs.xml")

        If IO.File.Exists(BaseFileName) Then
            Dim SR As New Xml.Serialization.XmlSerializer(GetType(AppPrefs))
            Using Fr As New StreamReader(BaseFileName)
                Prefs = CType(SR.Deserialize(Fr), AppPrefs)
            End Using
        End If

        Return
    End Sub

    Public Sub save()
        Dim BaseFileName As String = IO.Path.Combine(RouteurModel.BaseFileDir, "AppPrefs.xml")

        Dim SR As New Xml.Serialization.XmlSerializer(GetType(AppPrefs))
        Using Fr As New StreamWriter(BaseFileName)
            SR.Serialize(Fr, Me)
        End Using


    End Sub

End Class
