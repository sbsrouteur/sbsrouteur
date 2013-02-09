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
Imports System.Xml.Serialization
Imports System.Xml
Imports System.Collections.ObjectModel

Public Class RacePrefs

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Const RACE_COURSE_EXTENSION_HOURS As Integer = 10

    Public Enum EnumMapLevels As Integer
        crude = 0
        low = 1
        intermediate = 2
        high = 3
        full = 4
    End Enum

    Public Enum RaceZoneDirs As Integer
        East = 0
        North = 1
        West = 2
        South = 3
    End Enum

    Public Enum RouterMode As Integer
        DTF = 0
        MIX_DTF_SPEED = 1
        MIX_DTF_ETA = 2
        ISO = 3
    End Enum


    Private _GridGrain As Double
    Private _MapLevel As EnumMapLevels
    Private _RaceOffset(3) As Double
    Private _Levels As List(Of EnumMapLevels)
    Private _RaceID As Integer
    Private _EllipseExtFactor As Double
    Private _CourseExtensionHours As Double
    Private _NoExclusionZone As Boolean
    Private _AutoRestartRouter As Boolean = False
    Private _FastRouteShortMeteo As Boolean = False
    Private _FastRouteShortPolar As Boolean = False
    Private _SaveRoute As Boolean = False
    Private _RouteurMode As RouterMode = RouterMode.ISO

    'IsoChrones Prefs
    Private _IsoLookupAngle As Double
    Private _IsoStep As TimeSpan
    Private _IsoStep_24 As TimeSpan
    Private _IsoStep_48 As TimeSpan
    Private _IsoAngleStep As Double

    'Routing Dest
    Private _UseCustomDest As Boolean = False
    Private _RouteDest As Coords

    'Routing Start
    Private _UseCustomStart As Boolean = False
    Private _RouteStart As Coords

    'Routing Start Date
    Private _UseCustomStartDate As Boolean = False
    Private _CustomStartDate As DateTime



    Public Shared Function GetRaceInfo(ByVal RaceID As String) As RacePrefs

        Dim RetValue As RacePrefs = Nothing
#If TESTING = 1 Then
        Dim fName As String = RouteurModel.BaseFileDir & "\T_ri_" & RaceID & ".xml"
#Else
        Dim fName As String = RouteurModel.BaseFileDir & "\ri_" & RaceID & ".xml"
#End If

        Dim xmlloaded As Boolean = False

        If System.IO.File.Exists(fName) Then
            Dim SR As System.IO.StreamReader = Nothing
            Try
                SR = New System.IO.StreamReader(fName)
                Dim Ser As New Xml.Serialization.XmlSerializer(GetType(RacePrefs))

                RetValue = CType(Ser.Deserialize(SR), RacePrefs)
                xmlloaded = True
            Catch ex As Exception
                MessageBox.Show("Error loading race info, fauling back to default. Check race preferences!!" & vbCrLf & ex.Message)
            Finally
                If Not SR Is Nothing Then
                    SR.Close()
                    SR = Nothing
                End If
            End Try
        End If

        If Not xmlloaded Then
            RetValue = New RacePrefs
            With RetValue
                .MapLevel = EnumMapLevels.intermediate
                .GridGrain = 0.5
                For i As Integer = 0 To 3
                    .RaceOffset(i) = 1
                Next
                .EllipseExtFactor = 1.3
                .CourseExtensionHours = RACE_COURSE_EXTENSION_HOURS
                .IsoLookupAngle = 90
                .IsoAngleStep = 5
                .UseCustomDest = False
                .RouteDest = Nothing
                .UseCustomStartDate = False
                .CustomStartDate = Now
            End With

        End If

        Return RetValue

    End Function

    Public Property AutoRestartRouter() As Boolean
        Get
            Return _AutoRestartRouter
        End Get
        Set(ByVal value As Boolean)
            _AutoRestartRouter = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AutoRestartRouter"))
        End Set
    End Property

    Public Property CourseExtensionHours() As Double
        Get
            Return _CourseExtensionHours
        End Get
        Set(ByVal value As Double)

            If _CourseExtensionHours <> value Then
                _CourseExtensionHours = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CourseExtensionHours"))
            End If
        End Set
    End Property

    <XmlAttribute("CustomStartDate")> _
    Public Property CustomStartDate() As DateTime
        Get
            Return _CustomStartDate
        End Get
        Set(ByVal value As DateTime)
            _CustomStartDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CustomStartDate"))
        End Set
    End Property

    Public Property EllipseExtFactor() As Double
        Get
            Return _EllipseExtFactor
        End Get
        Set(ByVal value As Double)
            If _EllipseExtFactor <> value Then
                _EllipseExtFactor = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("EllipseExtFactor"))
            End If

        End Set
    End Property

    Public Property FastRouteShortMeteo() As Boolean
        Get
            Return _FastRouteShortMeteo
        End Get
        Set(ByVal value As Boolean)
            _FastRouteShortMeteo = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("FastRouteShortMeteo"))
        End Set
    End Property

    Public Property FastRouteShortPolar() As Boolean
        Get
            Return _FastRouteShortPolar
        End Get
        Set(ByVal value As Boolean)
            _FastRouteShortPolar = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("FastRouteShortPolar"))
        End Set
    End Property

    Public Property GridGrain() As Double
        Get
            Return _GridGrain
        End Get
        Set(ByVal value As Double)

            If _GridGrain <> value Then
                _GridGrain = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("GridGrain"))
            End If
        End Set
    End Property


    Public Property IsoAngleStep() As Double
        Get
            Return _IsoAngleStep
        End Get
        Set(ByVal value As Double)
            If value <> _IsoAngleStep Then
                _IsoAngleStep = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsoAngleStep"))
            End If

        End Set
    End Property

    Public Property IsoLookupAngle() As Double
        Get
            Return _IsoLookupAngle
        End Get
        Set(ByVal value As Double)
            If value <> _IsoLookupAngle Then
                _IsoLookupAngle = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsoLookupAngle"))
            End If
        End Set
    End Property

    <XmlAttribute("IsoStep")> _
    Public Property IsoStepString() As String
        Get
            Return XmlConvert.ToString(_IsoStep)
        End Get
        Set(ByVal value As String)
            _IsoStep = XmlConvert.ToTimeSpan(value)
        End Set
    End Property

    <XmlAttribute("IsoStep_24")> _
    Public Property IsoStep24String() As String
        Get
            Return XmlConvert.ToString(_IsoStep_24)
        End Get
        Set(ByVal value As String)
            _IsoStep_24 = XmlConvert.ToTimeSpan(value)
        End Set
    End Property

    <XmlAttribute("IsoStep_48")> _
    Public Property IsoStep48String() As String
        Get
            Return XmlConvert.ToString(_IsoStep_48)
        End Get
        Set(ByVal value As String)
            _IsoStep_48 = XmlConvert.ToTimeSpan(value)
        End Set
    End Property

    Public Property MapLevel() As EnumMapLevels
        Get
            Return _MapLevel
        End Get
        Set(ByVal value As EnumMapLevels)
            If _MapLevel <> value Then
                _MapLevel = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MapLevel"))
            End If

        End Set
    End Property

    Public Property NoExclusionZone() As Boolean
        Get
            Return _NoExclusionZone
        End Get
        Set(ByVal value As Boolean)
            _NoExclusionZone = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NoExclusionZone"))
        End Set
    End Property

    <XmlIgnore()> _
    Public Property RaceOffset() As Double()
        Get
            Return _RaceOffset
        End Get
        Set(ByVal value As Double())
            If value IsNot _RaceOffset Then
                _RaceOffset = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceZoneOffset"))
            End If
        End Set
    End Property

    Public Property RouteDest() As Coords
        Get
            Return _RouteDest
        End Get
        Set(ByVal value As Coords)
            _RouteDest = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteDest"))
        End Set
    End Property

    Public Property RouteStart() As Coords
        Get
            Return _RouteStart
        End Get
        Set(ByVal value As Coords)
            _RouteStart = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteStart"))
        End Set
    End Property

    Public Property RouteurMode As RouterMode
        Get
            Return _RouteurMode
        End Get
        Set(value As RouterMode)
            _RouteurMode = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteurMode"))
        End Set
    End Property

    Public ReadOnly Property RouteurModes As ObservableCollection(Of RouterMode)
        Get
            Dim ret As New ObservableCollection(Of RouterMode)
            ret.Add(RouterMode.DTF)
            ret.Add(RouterMode.MIX_DTF_SPEED)
            ret.Add(RouterMode.MIX_DTF_ETA)
            ret.Add(RouterMode.ISO)

            Return ret
        End Get
    End Property

    Public Property SaveRoute() As Boolean
        Get
            Return _SaveRoute
        End Get
        Set(ByVal value As Boolean)
            _SaveRoute = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SaveRoute"))
        End Set
    End Property

    Public Property SouthOffset() As Double
        Get
            Return _RaceOffset(RaceZoneDirs.South)
        End Get
        Set(ByVal value As Double)
            _RaceOffset(RaceZoneDirs.South) = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceZoneOffset"))

        End Set
    End Property

    Public Property EastOffset() As Double
        Get
            Return _RaceOffset(RaceZoneDirs.East)
        End Get
        Set(ByVal value As Double)
            _RaceOffset(RaceZoneDirs.East) = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceZoneOffset"))

        End Set
    End Property

    Public Property UseCustomDest() As Boolean
        Get
            Return _UseCustomDest
        End Get
        Set(ByVal value As Boolean)
            _UseCustomDest = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UseCustomDest"))
        End Set
    End Property

    Public Property UseCustomStart() As Boolean
        Get
            Return _UseCustomStart
        End Get
        Set(ByVal value As Boolean)
            _UseCustomStart = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UseCustomStart"))
        End Set
    End Property

    Public Property UseCustomStartDate() As Boolean
        Get
            Return _UseCustomStartDate
        End Get
        Set(ByVal value As Boolean)
            _UseCustomStartDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UseCustomStartDate"))
        End Set
    End Property

    Public Property WestOffset() As Double
        Get
            Return _RaceOffset(RaceZoneDirs.West)
        End Get
        Set(ByVal value As Double)
            _RaceOffset(RaceZoneDirs.West) = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceZoneOffset"))

        End Set
    End Property

    Public Property NorthOffset() As Double
        Get
            Return _RaceOffset(RaceZoneDirs.North)
        End Get
        Set(ByVal value As Double)
            _RaceOffset(RaceZoneDirs.North) = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceZoneOffset"))

        End Set
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property ListMapLevels() As List(Of EnumMapLevels)
        Get
            If _Levels Is Nothing Then

                Dim Files() As String = System.IO.Directory.GetFiles("..\gshhs", "gshhs_*.b")
                _Levels = New List(Of EnumMapLevels)
                For Each F In Files

                    Dim MapCode As String = F.ToString.Substring(15, 1)
                    For Each value As EnumMapLevels In [Enum].GetValues(GetType(EnumMapLevels))
                        If value.ToString.StartsWith(MapCode) Then
                            _Levels.Add(value)
                            Exit For
                        End If
                    Next

                Next

            End If

            Return _Levels

        End Get
    End Property

    Public Shared Sub Save(ByVal RaceID As String, ByVal Data As RacePrefs)

#If TESTING = 1 Then
        Dim fName As String = RouteurModel.BaseFileDir & "\T_ri_" & RaceID & ".xml"
#Else
        Dim fName As String = RouteurModel.BaseFileDir & "\ri_" & RaceID & ".xml"
#End If

        Dim Ser As New System.Xml.Serialization.XmlSerializer(Data.GetType)

        If Not System.IO.Directory.Exists(RouteurModel.BaseFileDir) Then
            System.IO.Directory.CreateDirectory(RouteurModel.BaseFileDir)
        End If

        Dim SR As New System.IO.StreamWriter(fName)
        Ser.Serialize(SR, Data)
        SR.Close()

    End Sub



End Class
