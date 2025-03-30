module App.FunctionalUtils

let intersperse separator xs =
    [ if List.isEmpty xs then
          ()
      else
          yield xs[0]

          for x in xs[1..] do
              yield separator
              yield x ]

let dropLast (xs: 'T list) = xs[.. xs.Length - 2]
