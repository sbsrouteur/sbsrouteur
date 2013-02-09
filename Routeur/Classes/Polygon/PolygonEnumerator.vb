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

Public Class PolygonEnumerator

    Implements IEnumerator(Of Coords)


    Private _CurIndex As Integer
    Private _Poly As Polygon

    Public Sub New(ByVal P As Polygon)
        _Poly = P
        _CurIndex = -1
    End Sub

    Public ReadOnly Property Current() As Coords Implements System.Collections.Generic.IEnumerator(Of Coords).Current
        Get
            If Not _Poly Is Nothing Then
                Return _Poly(_CurIndex)
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
        If _CurIndex < _Poly.Count Then
            _CurIndex += 1
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub Reset() Implements System.Collections.IEnumerator.Reset
        _CurIndex = -1
    End Sub

    Private disposedValue As Boolean = False        ' Pour détecter les appels redondants

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO : libérez un autre état (objets managés).
            End If

            ' TODO : libérez votre propre état (objets non managés).
            ' TODO : définissez les champs volumineux à null.
        End If
        Me.disposedValue = True
    End Sub

#Region " IDisposable Support "
    ' Ce code a été ajouté par Visual Basic pour permettre l'implémentation correcte du modèle pouvant être supprimé.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Ne modifiez pas ce code. Ajoutez du code de nettoyage dans Dispose(ByVal disposing As Boolean) ci-dessus.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Public ReadOnly Property Current1() As Object Implements System.Collections.IEnumerator.Current
        Get
            Return Current()
        End Get
    End Property


End Class
