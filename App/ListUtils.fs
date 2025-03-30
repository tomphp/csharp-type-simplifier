module ListUtils

let intersperse (separator: 'T) : 'T list -> 'T list =
    function
    | [] -> []
    | xs ->
        [ yield xs[0]
          for x in xs[1..] do
              yield separator
              yield x ]
