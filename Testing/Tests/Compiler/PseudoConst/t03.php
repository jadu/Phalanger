[EXPECT] g

[FILE]
<?
class A
{
  static function f()
  {
    class B
    {
      static function g() { echo __FUNCTION__; }
    }
    B::g();
  }
}

A::f();

?>
