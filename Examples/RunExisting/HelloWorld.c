#define WASM_EXPORT __attribute__((visibility("default")))

/* External function that is implemented in JavaScript. */
extern void sayc(char c);

int say_something(char *msg) {
  char *m = msg;
  while (*m)
    sayc(*(m++));

  return m - msg;
}

WASM_EXPORT
int main(void) {
  say_something("Hello World (from WASM)\n");
}
