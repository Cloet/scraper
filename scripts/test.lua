local treat = require('treat')
function main(splash)
    splash.js_enabled=false
    assert(splash:go(splash.args.url))
    assert(splash:wait(0.5))

    local imgs = splash:select_all('button')
    local srcs = {}

    for _, img in ipairs(imgs) do
      srcs[#srcs+1] = img.node.attributes.class
    end

    return treat.as_array(srcs)
end