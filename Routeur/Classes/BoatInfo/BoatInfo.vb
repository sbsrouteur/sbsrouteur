Imports System.ComponentModel
Imports System.IO
Imports System.Net

Public Class BoatInfo

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _CurPos As New Coords
    Private _Name As String
    Public Drawn As Boolean = False
    Public ProOption As Boolean
    Private _Classement As Integer
    Public Temps As DateTime
    Public TotalPoints As Decimal
    Private _Flag As BitmapImage
    Private _FlagName As String
    Private _Last1H As Double
    Private _Last3H As Double
    Private _LastDTF As Double
    Private _Dtf As Double
    Private _TimeToPass As Double
    Private _PassUp As Boolean

    Private Shared _ImgList As New SortedList(Of String, BitmapImage)

    Public Property Classement() As Integer
        Get
            Return _Classement
        End Get
        Set(ByVal value As Integer)
            _Classement = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Classement"))
        End Set
    End Property

    Public Property CurPos() As Coords
        Get
            Return _CurPos
        End Get
        Set(ByVal value As Coords)
            _CurPos = value
        End Set
    End Property

    Public Property Dtf() As Double
        Get
            Return _Dtf
        End Get
        Set(ByVal value As Double)
            _Dtf = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Dtf"))
        End Set
    End Property

    Public ReadOnly Property Flag() As BitmapImage
        Get
            Return ImgList(FlagName)
        End Get
    End Property

    Public Property FlagName() As String
        Get
            Return _FlagName
        End Get
        Set(ByVal value As String)
            _FlagName = value
        End Set
    End Property

    Public Shared ReadOnly Property ImgList() As SortedList(Of String, BitmapImage)
        Get
            Return _ImgList
        End Get
    End Property

    Public Shared Property ImgList(ByVal Index As String) As BitmapImage
        Get
            If _ImgList(Index) Is Nothing OrElse _ImgList(Index).Width < 10 Then
                Dim FlagsPath As String = Path.Combine(RouteurModel.BaseFileDir, "Flags")
                If Not Directory.Exists(FlagsPath) Then
                    Directory.CreateDirectory(FlagsPath)
                End If
                FlagsPath = Path.Combine(FlagsPath, Index)
                If Not File.Exists(FlagsPath) Then

                    Using wc As New WebClient
                        wc.DownloadFile(RouteurModel.Base_Game_Url & "/flagimg.php?idflags=" & Index, FlagsPath)
                    End Using
                End If
                Dim img As New BitmapImage(New Uri("File://" & FlagsPath))
                _ImgList(Index) = img

            End If

            Return _ImgList(Index)
        End Get
        Set(ByVal value As BitmapImage)
            _ImgList(Index) = value
        End Set
    End Property

    Public Property Last1H() As Double
        Get
            Return _Last1H
        End Get
        Set(ByVal value As Double)
            _Last1H = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Last1H"))
        End Set
    End Property

    Public Property Last3H() As Double
        Get
            Return _Last3H
        End Get
        Set(ByVal value As Double)
            _Last3H = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Last3H"))
        End Set
    End Property

    Public Property LastDTF() As Double
        Get
            Return _LastDTF
        End Get
        Set(ByVal value As Double)
            _LastDTF = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LastDTF"))
        End Set
    End Property
    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
        End Set
    End Property

    Public Property PassUp() As Boolean
        Get
            Return _PassUp
        End Get
        Set(ByVal value As Boolean)
            _PassUp = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PassUp"))
        End Set
    End Property

    Public Property TimeToPass() As Double
        Get
            Return _TimeToPass
        End Get
        Set(ByVal value As Double)
            _TimeToPass = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TimeToPass"))
        End Set
    End Property



End Class