Imports System.ComponentModel
Imports System.Xml.Serialization

Public Class RacePrefs

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

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
                    .RaceOffset(i) = 0
                Next
                .EllipseExtFactor = 1.3
            End With

        End If

        Return RetValue

    End Function

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

    Public Property MapLevel() As EnumMapLevels
        Get
            Return _MapLevel
        End Get
        Set(ByVal value As EnumMapLevels)
            If _MapLevel <> value Then
                _MapLevel = value
            End If
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MapLevel"))

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
