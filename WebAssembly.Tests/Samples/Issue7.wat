(module
  (type $t0 (func (param i32)))
  (type $t1 (func))
  (type $t2 (func (result i32)))
  (import "env" "sayc" (func $sayc (type $t0)))
  (func $__wasm_call_ctors (type $t1))
  (func $main (export "main") (type $t2) (result i32)
    i32.const 72
    call $sayc
    i32.const 101
    call $sayc
    i32.const 108
    call $sayc
    i32.const 108
    call $sayc
    i32.const 111
    call $sayc
    i32.const 32
    call $sayc
    i32.const 87
    call $sayc
    i32.const 111
    call $sayc
    i32.const 114
    call $sayc
    i32.const 108
    call $sayc
    i32.const 100
    call $sayc
    i32.const 32
    call $sayc
    i32.const 40
    call $sayc
    i32.const 102
    call $sayc
    i32.const 114
    call $sayc
    i32.const 111
    call $sayc
    i32.const 109
    call $sayc
    i32.const 32
    call $sayc
    i32.const 87
    call $sayc
    i32.const 65
    call $sayc
    i32.const 83
    call $sayc
    i32.const 77
    call $sayc
    i32.const 41
    call $sayc
    i32.const 10
    call $sayc
    i32.const 0)
  (table $T0 1 1 anyfunc)
  (memory $memory (export "memory") 2)
  (global $g0 (mut i32) (i32.const 66560))
  (global $__heap_base (export "__heap_base") i32 (i32.const 66560))
  (global $__data_end (export "__data_end") i32 (i32.const 1024)))
