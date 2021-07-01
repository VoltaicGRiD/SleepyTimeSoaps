namespace SleepyTimeSoaps.Models
{
    public class JavaScriptStrings
    {
        public static string AddToCartSuccess = @"<script>let timerInterval
                                                Swal.fire({
                                                  title: 'Successfully added to your bag!',
                                                  timer: 3000,
                                                  timerProgressBar: true,
                                                  didOpen: () => {
                                                    Swal.showLoading()
                                                    timerInterval = setInterval(() => {
                                                      const content = Swal.getContent()
                                                      if (content) {
                                                        const b = content.querySelector('b')
                                                        if (b) {
                                                          b.textContent = Swal.getTimerLeft()
                                                        }
                                                      }
                                                    }, 100)
                                                  },
                                                  willClose: () => {
                                                    clearInterval(timerInterval)
                                                  }
                                                }).then((result) => {
                                                  /* Read more about handling dismissals below */
                                                  if (result.dismiss === Swal.DismissReason.timer) {
                                                    console.log('I was closed by the timer')
                                                  }
                                                })</script>";
    }
}