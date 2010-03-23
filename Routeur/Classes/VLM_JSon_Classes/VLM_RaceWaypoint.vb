Public Class VLM_RaceWaypoint

    Private _wporder As Integer
    Private _laisser_au As Integer
    Private _wptype As String
    Private _latitude1 As Integer
    Private _longitude1 As Integer
    Private _latitude2 As Integer
    Private _longitude2 As Integer
    Private _libelle As String
    Private _maparea As Integer


    Public Property Laisser_au() As Integer
        Get
            Return _laisser_au
        End Get
        Set(ByVal value As Integer)
            _laisser_au = value
        End Set
    End Property

    Public Property Latitude1() As Integer
        Get
            Return _latitude1
        End Get
        Set(ByVal value As Integer)
            _latitude1 = value
        End Set
    End Property

    Public Property Latitude2() As Integer
        Get
            Return _latitude2
        End Get
        Set(ByVal value As Integer)
            _latitude2 = value
        End Set
    End Property

    Public Property Libelle() As String
        Get
            Return _libelle
        End Get
        Set(ByVal value As String)
            _libelle = value
        End Set
    End Property

    Public Property Longitude1() As Integer
        Get
            Return _longitude1
        End Get
        Set(ByVal value As Integer)
            _longitude1 = value
        End Set
    End Property

    Public Property Longitude2() As Integer
        Get
            Return _longitude2
        End Get
        Set(ByVal value As Integer)
            _longitude2 = value
        End Set
    End Property

    Public Property Maparea() As Integer
        Get
            Return _maparea
        End Get
        Set(ByVal value As Integer)
            _maparea = value
        End Set
    End Property

    Public Property Wporder() As Integer
        Get
            Return _wporder
        End Get
        Set(ByVal value As Integer)
            _wporder = value
        End Set
    End Property

    Public Property Wptype() As String
        Get
            Return _wptype
        End Get
        Set(ByVal value As String)
            _wptype = value
        End Set
    End Property

End Class
