function wait_for_element(splash, css, text, maxwait)
  -- Wait until a selector matches an element
  -- in the page. Return an error if waited more
  -- than maxwait seconds.
  if maxwait == nil then
      maxwait = 10
  end
  return splash:wait_for_resume(string.format([[
    function main(splash) {
      var selector = '%s';
      var maxwait = %s;
      var end = Date.now() + maxwait*1000;

      function check() {
        if(document.querySelector(selector)) {
          document.querySelectorAll(selector)[0].click();
          splash.resume('Element found');
        } else if(Date.now() >= end) {
          var err = 'Timeout waiting for element';
          splash.error(err + " " + selector);
        } else {
          setTimeout(check, 200);
        }
      }
      check();
    }
  ]], css, maxwait))
end

function main(splash, args)
  splash:go(args.url)
  wait_for_element(splash, '.cmc-button:contains("Load More")')
  return {splash:html()}
end



-- function main(splash, args)
--   splash.js_enabled = false
--   assert(splash:go(args.url))
--   assert(splash:wait(10))
--   return {
--     html = splash:html(),
--     png = splash:png(),
--     har = splash:har(),
--   }
-- end

-- function main(splash, args)
--     splash:go(args.url)
--     assert(splash:wait(0.5))
--     local btns = splash:select_all('.cmc-button')
--     local test = "no"
--     for _, btn in ipairs(btns) do
--         ok, value = btn:field_value()
--         if (ok) then
--             test = test .. value
--         end
--     end

--     return {test}
-- end