Imports System.ComponentModel
Imports System.Xml.Serialization
Imports System.Xml

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


    Private _GridGrain As Double
    Private _MapLevel As EnumMapLevels
    Private _RaceOffset(3) As Double
    Private _Levels As List(Of EnumMapLevels)
    Private _RaceID As Integer
    Private _EllipseExtFactor As Double
    Private _CourseExtensionHours As Double

    'IsoChrones Prefs
    Private _IsoLookupAngle As Double
    Private _IsoStep As TimeSpan
    Private _IsoStep_24 As TimeSpan
    Private _IsoStep_48 As TimeSpan
    Private _IsoAngleStep As Double


    Public Shared Function GetRaceInfo(ByVal RaceID As Integer) As RacePrefs

        Dim RetValue As RacePrefs = Nothing
        Dim fName As String = RouteurModel.BaseFileDir & "\ri_" & RaceID & ".xml"
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
                .GridGrain = 0.01
                For i As Integer = 0 To 3
                    .RaceOffset(i) = 0.5
                Next
                .EllipseExtFactor = 1.3
                .CourseExtensionHours = RACE_COURSE_EXTENSION_HOURS
                .IsoLookupAngle = 60
                .IsoAngleStep = 3
                .IsoStep = New TimeSpan(1, 0, 0)
                .IsoStep_24 = New TimeSpan(3, 0, 0)
                .IsoStep_48 = New TimeSpan(12, 0, 0)
            End With

        End If

        Return RetValue

    End Function

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

    <XmlIgnore()> _
    Public Property IsoStep() As TimeSpan
        Get
            Return _IsoStep
        End Get
        Set(ByVal value As TimeSpan)
            If value.Ticks <> _IsoStep.Ticks Then
                _IsoStep = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsoStep"))
            End If
        End Set
    End Property

    <XmlIgnore()> _
    Public Property IsoStep_24() As TimeSpan
        Get
            Return _IsoStep_24
        End Get
        Set(ByVal value As TimeSpan)
            If value.Ticks <> _IsoStep_24.Ticks Then
                _IsoStep_24 = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsoStep_24"))
            End If
        End Set
    End Property

    <XmlIgnore()> _
    Public Property IsoStep_48() As TimeSpan
        Get
            Return _IsoStep_48
        End Get
        Set(ByVal value As TimeSpan)
            If value.Ticks <> _IsoStep_48.Ticks Then
                _IsoStep_48 = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsoStep_48"))
            End If
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

    Public Shared Sub Save(ByVal RaceID As Integer, ByVal Data As RacePrefs)

        Dim fName As String = RouteurModel.BaseFileDir & "\ri_" & RaceID & ".xml"
        Dim Ser As New System.Xml.Serialization.XmlSerializer(Data.GetType)

        If Not System.IO.Directory.Exists(RouteurModel.BaseFileDir) Then
            System.IO.Directory.CreateDirectory(RouteurModel.BaseFileDir)
        End If

        Dim SR As New System.IO.StreamWriter(fName)
        Ser.Serialize(SR, Data)
        SR.Close()

    End Sub



End Class
