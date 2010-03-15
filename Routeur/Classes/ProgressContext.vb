Imports System.ComponentModel

Public MustInherit Class ProgressContext

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _ProgressValue As Double
    Private _ProgressETA As TimeSpan
    Private _Dirty As Boolean = False

    Private _Title As String

    Public Property Dirty() As Boolean
        Get
            Return _Dirty
        End Get
        Set(ByVal value As Boolean)
            _Dirty = value
        End Set
    End Property

    Public Property ProgressETA() As TimeSpan
        Get
            Return _ProgressETA
        End Get
        Set(ByVal value As TimeSpan)

            If _ProgressETA <> value Then
                _ProgressETA = value
                Dirty = True
            End If
        End Set
    End Property

    Public Property ProgressValue() As Double
        Get
            Return _ProgressValue
        End Get
        Set(ByVal value As Double)

            If value <> _ProgressValue Then
                _ProgressValue = value
                Dirty = True
            End If

        End Set
    End Property

    Public Overridable Sub OnPropertyChanged(ByVal e As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(Me, e)

    End Sub

    Public Sub refresh()
        Dirty = False
        OnPropertyChanged(New PropertyChangedEventArgs("ProgressValue"))
        OnPropertyChanged(New PropertyChangedEventArgs("ProgressETA"))

    End Sub

    Public Property Title() As String
        Get
            Return _Title
        End Get
        Set(ByVal value As String)

            If Title <> value Then
                _Title = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Title"))
            End If
        End Set
    End Property


End Class


