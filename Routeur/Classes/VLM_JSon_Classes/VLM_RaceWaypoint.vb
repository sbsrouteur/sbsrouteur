Public Class VLM_RaceWaypoint

    Implements IComparable(Of VLM_RaceWaypoint)


    Private _wporder As Integer
    Private _laisser_au As Integer
    Private _wptype As String
    Private _latitude1 As Integer
    Private _longitude1 As Integer
    Private _latitude2 As Integer
    Private _longitude2 As Integer
    Private _libelle As String
    Private _maparea As Integer



    Public Property laisser_au() As Integer
        Get
            Return _laisser_au
        End Get
        Set(ByVal value As Integer)
            _laisser_au = value
        End Set
    End Property

    Public Property latitude1() As Integer
        Get
            Return _latitude1
        End Get
        Set(ByVal value As Integer)
            _latitude1 = value
        End Set
    End Property

    Public Property latitude2() As Integer
        Get
            Return _latitude2
        End Get
        Set(ByVal value As Integer)
            _latitude2 = value
        End Set
    End Property

    Public Property libelle() As String
        Get
            Return _libelle
        End Get
        Set(ByVal value As String)
            _libelle = value
        End Set
    End Property

    Public Property longitude1() As Integer
        Get
            Return _longitude1
        End Get
        Set(ByVal value As Integer)
            _longitude1 = value
        End Set
    End Property

    Public Property longitude2() As Integer
        Get
            Return _longitude2
        End Get
        Set(ByVal value As Integer)
            _longitude2 = value
        End Set
    End Property

    Public Property maparea() As Integer
        Get
            Return _maparea
        End Get
        Set(ByVal value As Integer)
            _maparea = value
        End Set
    End Property

    Public Property wporder() As Integer
        Get
            Return _wporder
        End Get
        Set(ByVal value As Integer)
            _wporder = value
        End Set
    End Property

    Public ReadOnly Property WPs() As List(Of Coords())
        Get
            Dim WP(1) As Coords
            Dim L As New List(Of Coords())
            WP(0) = New Coords(latitude1 / 1000, longitude1 / 1000)
            WP(1) = New Coords(latitude2 / 1000, longitude2 / 1000)
            L.Add(WP)
            Return L
        End Get
    End Property


    Public Property wptype() As String
        Get
            Return _wptype
        End Get
        Set(ByVal value As String)
            _wptype = value
        End Set
    End Property

    Public Function CompareTo(ByVal other As VLM_RaceWaypoint) As Integer Implements System.IComparable(Of VLM_RaceWaypoint).CompareTo

        If wporder = other.wporder Then
            Return 0
        ElseIf wporder > other.wporder Then
            Return 1
        Else
            Return -1
        End If

    End Function

    Public Overrides Function ToString() As String

        Return "WP " & wporder & " - " & libelle

    End Function

End Class
