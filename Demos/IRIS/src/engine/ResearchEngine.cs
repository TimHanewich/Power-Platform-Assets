using System;
using System.Threading;
using System.Threading.Tasks;
using TimHanewich.Bing;
using System.Net.Http;
using System.Net;
using TimHanewich.Bing.Search;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace PSJ
{
    public class ResearchEngine
    {

        public async Task<string> ResearchItemAsync(string item)
        {
            BingSearchService bss = new BingSearchService("<YOUR BING SEARCH API KEY HERE>");
            BingSearchResult[] results = await bss.SearchAsync(item + " wikipedia");


            string WebContent = "";
            HttpClient hc = new HttpClient();
            int pages_read = 0;
            bool wikipedia_accessed = false;
            foreach (BingSearchResult result in results)
            {
                if (pages_read < 3 && wikipedia_accessed == false)
                {
                    HttpResponseMessage resp = await hc.GetAsync(result.URL);
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        string html = await resp.Content.ReadAsStringAsync();
                        string text = HTMLToText(html);
                        WebContent = WebContent + text + Environment.NewLine + Environment.NewLine;

                        //Increment
                        pages_read = pages_read + 1;

                        //Did we just read from wikipedia?
                        if (result.URL.ToLower().Contains("wikipedia"))
                        {
                            wikipedia_accessed = true;
                        }
                    }
                }
            }


            //Trim down to character limit
            int CharacterLimit = 5000;
            WebContent = WebContent.Substring(0, CharacterLimit);
            
            //Prompt
            string prompt = WebContent + Environment.NewLine + Environment.NewLine + "Based on the above, describe " + item + " to me.";
            string response = await SimpleGPT.PromptAsync(prompt);


            return response;
        }



        //////// TOOLKIT BELOW //////

        public string HTMLToText(string HTMLCode)  
        {  
        // Remove new lines since they are not visible in HTML  
        HTMLCode = HTMLCode.Replace("\n", " ");  
        // Remove tab spaces  
        HTMLCode = HTMLCode.Replace("\t", " ");  
        // Remove multiple white spaces from HTML  
        HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");  
        // Remove HEAD tag  
        HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", ""  
                            , RegexOptions.IgnoreCase | RegexOptions.Singleline);  
        // Remove any JavaScript  
        HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", ""  
            , RegexOptions.IgnoreCase | RegexOptions.Singleline);  
        // Replace special characters like &, <, >, " etc.  
        StringBuilder sbHTML = new StringBuilder(HTMLCode);  
        // Note: There are many more special characters, these are just  
        // most common. You can add new characters in this arrays if needed  
        string[] OldWords = {"&nbsp;", "&amp;", "&quot;", "&lt;",  
    "&gt;", "&reg;", "&copy;", "&bull;", "&trade;","&#39;"};  
        string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "\'" };  
        for (int i = 0; i < OldWords.Length; i++)  
        {  
            sbHTML.Replace(OldWords[i], NewWords[i]);  
        }  
        // Check if there are line breaks (<br>) or paragraph (<p>)  
        sbHTML.Replace("<br>", "\n<br>");  
        sbHTML.Replace("<br ", "\n<br ");  
        sbHTML.Replace("<p ", "\n<p ");  
        // Finally, remove all HTML tags and return plain text  
        return System.Text.RegularExpressions.Regex.Replace(  
            sbHTML.ToString(), "<[^>]*>", "");  
        }  


    }
}