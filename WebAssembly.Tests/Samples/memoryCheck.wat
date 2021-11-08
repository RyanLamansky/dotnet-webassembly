(module
  (type $t0 (func))
  (type $t1 (func (param i32 i32) (result i32)))
  (import "env" "memory" (memory $mem 1))
  (func $__wasm_call_ctors (type $t0))
  (func $compute_CRC32_Simple (export "compute_CRC32_Simple") (type $t1) (param $p0 i32) (param $p1 i32) (result i32)
    (local $l0 i32) (local $l1 i32)
    block $B0
      get_local $p1
      i32.const 1
      i32.lt_s
      br_if $B0
      i32.const 0
      set_local $l0
      loop $L1
        get_local $p0
        i32.load8_u
        i32.const 24
        i32.shl
        get_local $l0
        i32.xor
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        tee_local $l0
        i32.const 1
        i32.shl
        tee_local $l1
        i32.const 79764919
        i32.xor
        get_local $l1
        get_local $l0
        i32.const 0
        i32.lt_s
        select
        set_local $l0
        get_local $p0
        i32.const 1
        i32.add
        set_local $p0
        get_local $p1
        i32.const -1
        i32.add
        tee_local $p1
        br_if $L1
      end
      get_local $l0
      return
    end
    i32.const 0)
  (table $T0 1 1 anyfunc)
  (global $g0 (mut i32) (i32.const 66560))
  (global $__heap_base (export "__heap_base") i32 (i32.const 66560))
  (global $__data_end (export "__data_end") i32 (i32.const 1024)))
