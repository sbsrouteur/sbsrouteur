Class MainWindow

    Public Property BmSource As WriteableBitmap


    Private _PointsList() As Point = New Point() {New Point(0 + Rnd(), 0 + Rnd()), New Point(100 + Rnd(), 73 + Rnd()), New Point(-100 + Rnd(), 1000 + Rnd()),
                                                  New Point(400 + Rnd(), 350 + Rnd()), New Point(17 + Rnd(), 43 + Rnd()), New Point(171 + Rnd(), 413 + Rnd()), New Point(1117 + Rnd(), -143 + Rnd()), New Point(171 + Rnd(), 43 + Rnd())}

    Public Sub New()

        ' Cet appel est requis par le concepteur.
        InitializeComponent()

        ' Ajoutez une initialisation quelconque après l'appel InitializeComponent().
        BmSource = New WriteableBitmap(1000, 1000, 96, 96, PixelFormats.Pbgra32, Nothing)
        Dim bfirstpoint As Boolean = True
        Dim PrevPoint As Point
        Dim Col() As Integer = New Integer() {&HFF00CC00, &HFFCC00CC}
        Dim index As Integer = 0
        For Each Point In _PointsList
            If Not bfirstpoint Then
                BmSource.DrawLine(CInt(PrevPoint.X), CInt(PrevPoint.Y), CInt(Point.X), CInt(Point.Y), Col(index))
                BmSource.DrawEllipseCentered(CInt(Point.X), CInt(Point.Y), 2, 2, Col(index))
            Else
                bfirstpoint = False
            End If
            PrevPoint = Point
            index = (index + 1) Mod 2

        Next

        Dest.Source = BmSource
    End Sub
End Class
