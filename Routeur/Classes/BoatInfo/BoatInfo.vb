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

Imports System.ComponentModel
Imports System.IO
Imports System.Net

Public Class BoatInfo

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Id As Long
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
    Private _PrevDTFDate As DateTime
    Private _CurDTFDate As DateTime
    Private _Dtf As Double
    Private _TimeToPass As TimeSpan
    Private _PassUp As Boolean
    Private _PassDown As Boolean
    Private _MyTeam As Boolean
    Private _Real As Boolean
    Private _deptime As Long

    'Private Shared _ImgList As New SortedList(Of String, BitmapImage)

    Public Property Classement() As Integer
        Get
            Return _Classement
        End Get
        Set(ByVal value As Integer)
            If _Classement <> value Then
                _Classement = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Classement"))
            End If
        End Set
    End Property

    Public Property CurDTFDate() As DateTime
        Get
            Return _CurDTFDate
        End Get
        Set(ByVal value As DateTime)
            _CurDTFDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurDTFDate"))
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

    Public Property deptime As Long
        Get
            Return _deptime
        End Get
        Set(value As Long)
            _deptime = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("deptime"))
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
            If _FlagName <> value Then
                _FlagName = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Flag"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("FlagName"))
            End If

        End Set
    End Property

    'Public Shared ReadOnly Property ImgList() As SortedList(Of String, BitmapImage)
    '    Get
    '        Return _ImgList
    '    End Get
    'End Property

    Public Property Id As Long
        Get
            Return _Id
        End Get
        Set(value As Long)
            _Id = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Id"))
        End Set
    End Property

    Public Shared Property ImgList(ByVal Index As String) As BitmapImage
        Get
            'Real have no flag!!
            If Index Is Nothing Then
                Return Nothing
            End If

            'If Not _ImgList.ContainsKey(Index) OrElse _ImgList(Index) Is Nothing OrElse _ImgList(Index).Width < 10 Then
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
            Return img

            'End If

            'Return Nothing '_ImgList(Index)
        End Get
        Set(ByVal value As BitmapImage)
            '_ImgList(Index) = value
        End Set
    End Property

    Public Property Last1H() As Double
        Get
            Return _Last1H
        End Get
        Set(ByVal value As Double)
            If _Last1H <> value Then
                _Last1H = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Last1H"))
            End If
        End Set
    End Property

    Public Property Last3H() As Double
        Get
            Return _Last3H
        End Get
        Set(ByVal value As Double)
            If _Last3H <> value Then
                _Last3H = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Last3H"))
            End If

        End Set
    End Property

    Public Property LastDTF() As Double
        Get
            Return _LastDTF
        End Get
        Set(ByVal value As Double)
            If _LastDTF <> value Then
                _LastDTF = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LastDTF"))
            End If

        End Set
    End Property

    Public Property MyTeam() As Boolean
        Get
            Return _MyTeam
        End Get
        Set(value As Boolean)
            If _MyTeam <> value Then
                _MyTeam = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MyTeam"))
            End If
        End Set
    End Property

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            If Name <> value Then
                _Name = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
            End If

        End Set
    End Property

    Public Property PassDown() As Boolean
        Get
            Return _PassDown
        End Get
        Set(ByVal value As Boolean)
            If _PassDown <> value Then
                _PassDown = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PassUp"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PassVisibility"))
            End If
        End Set
    End Property

    Public Property PassUp() As Boolean
        Get
            Return _PassUp
        End Get
        Set(ByVal value As Boolean)
            If _PassUp <> value Then
                _PassUp = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PassUp"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PassVisibility"))
            End If

        End Set
    End Property

    Public ReadOnly Property PassVisibility As Boolean
        Get
            Return PassDown Or PassUp
        End Get
    End Property

    Public Property PrevDTFDate() As DateTime
        Get
            Return _PrevDTFDate
        End Get
        Set(ByVal value As DateTime)
            _PrevDTFDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PrevDTFDate"))
        End Set
    End Property

    Public ReadOnly Property RaceDepTime As DateTime
        Get
            Return New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(deptime).ToLocalTime
        End Get
    End Property

    Public Property Real As Boolean
        Get
            Return _Real
        End Get
        Set(value As Boolean)
            _Real = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Real"))
        End Set
    End Property


    Public Property TimeToPass() As TimeSpan
        Get
            Return _TimeToPass
        End Get
        Set(ByVal value As TimeSpan)
            If _TimeToPass.Ticks <> value.Ticks Then
                _TimeToPass = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TimeToPass"))
            End If
        End Set
    End Property



End Class